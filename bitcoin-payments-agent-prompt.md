# AI Agent Context Prompt — Bitcoin Payment Processing Platform

> **Purpose**: This document is the single source of truth for an AI coding agent building this project. It contains architecture decisions, implementation requirements, domain context, and testing strategy. Treat every section as a binding specification unless explicitly marked as optional. You are acting as a Senior Software Architect with deep expertise in both payment systems and Bitcoin protocol-level development. This is a production platform — build it like it ships.

---

## 1. Project Overview

### 1.1 What We're Building

A production-grade .NET Bitcoin payment processing platform that implements the full lifecycle of traditional payment operations — Charge, Authorization, Capture, Void, and Refund — using native Bitcoin primitives. This system bridges the established mental model of card-based payment gateways (auth/capture, voids, refunds) with Bitcoin's UTXO-based transaction model and script system.

The platform exposes a clean API surface that any merchant integration can consume, abstracting away the complexity of Bitcoin transaction construction, script management, and UTXO tracking behind familiar payment semantics.

### 1.2 Business Context

Traditional payment processors (Stripe, Chase Paymentech Orbital, Adyen, etc.) expose a standardized set of operations: Charge (sale), Auth (hold), Capture (settle a hold), Void (cancel before settlement), and Refund (return funds after settlement). Merchants and their integration partners understand and depend on this model.

Bitcoin has no native concept of "authorization holds" or "voids" — transactions are either unconfirmed, confirmed, or non-existent. This platform solves that gap by implementing payment-layer abstractions on top of Bitcoin's scripting capabilities (multisig, timelocks, HTLCs) so that merchants adopting crypto payments can work with the same operational model they already know.

### 1.3 Non-Negotiable Constraints

- **.NET 8+ (LTS)** — all application code targets .NET 8 or later.
- **NBitcoin** — the sole Bitcoin library. No other Bitcoin/crypto libraries.
- **Bitcoin Testnet4** — all blockchain interactions target Testnet4 for the initial deployment environment. The codebase must make it structurally impossible to accidentally target mainnet (e.g., `Network.Main` should never appear anywhere except a guard clause that throws).
- **No real funds** — the initial deployment operates against testnet. Any configuration that could route to mainnet must fail loudly at startup.
- **PostgreSQL** — persistent storage for transaction state, UTXO tracking, wallet metadata.
- **Clean Architecture** — Domain, Application, Infrastructure, API layers with strict dependency direction.
- **Idempotency** — every API operation must be idempotent. Payment systems cannot tolerate duplicate processing.

---

## 2. Domain Model — Payment Operations Mapped to Bitcoin

This is the core intellectual work of the platform. Each traditional payment operation has a specific Bitcoin implementation strategy.

### 2.1 Charge (Sale)

**Traditional meaning**: A single-step operation that authorizes and captures funds simultaneously. Money moves from buyer to merchant.

**Bitcoin implementation**: A standard on-chain transaction. Construct a transaction that spends the buyer's UTXOs and sends the specified amount to the merchant's receiving address, with change returned to the buyer's change address.

**NBitcoin flow**:
1. Select UTXOs from the buyer's tracked UTXO set (coin selection).
2. Build a `Transaction` using `TransactionBuilder`.
3. Sign with the buyer's key(s).
4. Broadcast to the Bitcoin testnet network.
5. Track confirmation status (0-conf → 1-conf → 6-conf).

**State transitions**: `Created → Broadcasting → Mempool → Confirming(n) → Settled`

### 2.2 Authorization (Auth / Hold)

**Traditional meaning**: Place a hold on funds. The money is reserved but has not moved to the merchant yet. The merchant has a window (typically 7–30 days in card processing) to capture or void.

**Bitcoin implementation**: Bitcoin has no native hold mechanism. We implement this using a **2-of-2 multisig escrow**:

1. Generate a new escrow address — a P2WSH (Pay-to-Witness-Script-Hash) address backed by a 2-of-2 multisig `RedeemScript` requiring signatures from both the buyer and the platform/merchant.
2. The buyer funds this escrow address with a standard transaction (the "funding tx").
3. **Before broadcasting the funding tx**, construct and pre-sign a timelock refund transaction that returns funds to the buyer after N blocks (the auth expiry window). This protects the buyer if capture never happens.
4. Once the funding tx confirms, the authorization is active.

**The escrow RedeemScript**:
```
OP_2 <buyer_pubkey> <merchant_pubkey> OP_2 OP_CHECKMULTISIG
```

**The timelock refund script** (pre-signed safety net):
```
<locktime> OP_CHECKLOCKTIMEVERIFY OP_DROP <buyer_pubkey> OP_CHECKSIG
```

**State transitions**: `Created → FundingBroadcast → FundingConfirmed (Auth Active) → [Captured | Voided | Expired]`

**Configuration**: Auth expiry window should be configurable (default: 144 blocks ≈ 1 day on mainnet, faster on testnet).

### 2.3 Capture

**Traditional meaning**: Settle a previously authorized hold. The merchant claims the funds.

**Bitcoin implementation**: Both the buyer and merchant (or platform on merchant's behalf) sign a transaction that spends the escrow UTXO and sends funds to the merchant's address.

**NBitcoin flow**:
1. Look up the auth record → get the escrow UTXO, redeem script, and pre-stored partial signatures.
2. Build a transaction spending the escrow UTXO to the merchant's receiving address.
3. Sign with merchant key → sign with buyer key (or use pre-signed buyer component from auth step).
4. Broadcast the capture transaction.

**Partial capture**: Support capturing less than the full authorized amount. The remainder is returned to the buyer as change from the escrow spend. This is a single transaction with two outputs: merchant amount + buyer change.

**State transitions**: `Auth Active → CaptureBroadcast → CaptureConfirming(n) → Settled`

### 2.4 Void

**Traditional meaning**: Cancel an authorization before capture. No funds should move to the merchant.

**Bitcoin implementation**: Depends on the state of the auth:

- **If funding tx is unconfirmed**: Attempt a Replace-By-Fee (RBF) transaction that redirects the escrow funds back to the buyer. This is only possible if the original funding tx was marked as RBF-eligible (sequence number < 0xFFFFFFFE).
- **If funding tx is confirmed but within auth window**: Both parties sign a cooperative close transaction that returns funds to the buyer (similar to capture but with buyer as the recipient).
- **If auth has expired (timelock elapsed)**: The pre-signed timelock refund can be broadcast by the buyer unilaterally. The platform should detect expired auths and trigger this automatically.

**State transitions**: `Auth Active → VoidBroadcast → VoidConfirming(n) → Voided`

### 2.5 Refund

**Traditional meaning**: Return funds to the buyer after a completed charge or capture. This is a post-settlement operation.

**Bitcoin implementation**: A new, independent on-chain transaction from the merchant's wallet to the buyer's original address (or a refund address provided by the buyer).

**Key differences from traditional refunds**:
- There is no protocol-level link between the original charge and the refund. The linkage exists only in the application's database.
- The refund amount may differ from the original due to network fees (the merchant typically absorbs the fee cost of the refund tx).
- Partial refunds are supported — just send less than the original amount.

**State transitions**: `Created → Broadcasting → Mempool → Confirming(n) → Settled`

---

## 3. Technical Architecture

### 3.1 Solution Structure

```
src/
├── BitcoinPayments.Domain/            # Entities, value objects, domain events, interfaces
│   ├── Entities/
│   │   ├── PaymentTransaction.cs      # Aggregate root — represents any payment operation
│   │   ├── Wallet.cs                  # HD wallet with key derivation
│   │   ├── Escrow.cs                  # Escrow state for auth/capture flows
│   │   └── Utxo.cs                    # Tracked UTXO
│   ├── ValueObjects/
│   │   ├── BitcoinAddress.cs
│   │   ├── TransactionId.cs
│   │   ├── Money.cs                   # Wraps NBitcoin.Money with domain semantics
│   │   └── PaymentOperationType.cs    # Enum: Charge, Auth, Capture, Void, Refund
│   ├── Enums/
│   │   └── TransactionState.cs        # State machine states
│   ├── Events/
│   │   ├── PaymentCreated.cs
│   │   ├── PaymentBroadcast.cs
│   │   ├── PaymentConfirmed.cs
│   │   └── AuthExpired.cs
│   └── Interfaces/
│       ├── IWalletRepository.cs
│       ├── ITransactionRepository.cs
│       ├── IUtxoRepository.cs
│       ├── IBitcoinNetwork.cs         # Abstraction over network operations
│       └── IEscrowRepository.cs
│
├── BitcoinPayments.Application/       # Use cases, commands, queries, DTOs
│   ├── Commands/
│   │   ├── CreateChargeCommand.cs
│   │   ├── CreateAuthCommand.cs
│   │   ├── CaptureAuthCommand.cs
│   │   ├── VoidAuthCommand.cs
│   │   └── CreateRefundCommand.cs
│   ├── Handlers/
│   │   ├── ChargeHandler.cs
│   │   ├── AuthHandler.cs
│   │   ├── CaptureHandler.cs
│   │   ├── VoidHandler.cs
│   │   └── RefundHandler.cs
│   ├── Services/
│   │   ├── TransactionBuilderService.cs   # Wraps NBitcoin TransactionBuilder
│   │   ├── EscrowService.cs               # Manages escrow script construction
│   │   ├── CoinSelectionService.cs        # UTXO selection strategies
│   │   └── FeeEstimationService.cs        # Fee rate estimation
│   ├── DTOs/
│   │   ├── PaymentRequest.cs
│   │   ├── PaymentResponse.cs
│   │   ├── AuthorizationResponse.cs
│   │   └── TransactionStatusResponse.cs
│   └── Validators/
│       └── PaymentRequestValidator.cs     # FluentValidation
│
├── BitcoinPayments.Infrastructure/    # NBitcoin integration, DB, external services
│   ├── Bitcoin/
│   │   ├── NBitcoinNetworkService.cs      # Implements IBitcoinNetwork
│   │   ├── TestnetBroadcaster.cs          # Broadcasts via testnet node/API
│   │   ├── BlockchainMonitor.cs           # Watches for confirmations (background service)
│   │   ├── HdWalletManager.cs             # BIP32/BIP44 key derivation via NBitcoin
│   │   └── ScriptBuilder.cs              # Escrow, multisig, HTLC, timelock script construction
│   ├── Persistence/
│   │   ├── AppDbContext.cs                # EF Core DbContext
│   │   ├── Repositories/
│   │   │   ├── WalletRepository.cs
│   │   │   ├── TransactionRepository.cs
│   │   │   ├── UtxoRepository.cs
│   │   │   └── EscrowRepository.cs
│   │   └── Migrations/
│   ├── Configuration/
│   │   ├── BitcoinOptions.cs              # Network config, fee defaults, auth window
│   │   └── NetworkGuard.cs               # Startup check — throws if Network.Main detected
│   └── BackgroundServices/
│       ├── ConfirmationWatcher.cs         # Polls for tx confirmations, updates state
│       └── AuthExpiryMonitor.cs           # Detects expired auths, triggers auto-void
│
├── BitcoinPayments.API/              # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── PaymentsController.cs          # POST /payments/charge, /payments/auth, etc.
│   │   ├── WalletsController.cs           # Wallet creation, balance, address generation
│   │   └── TransactionsController.cs      # Transaction status, history
│   ├── Middleware/
│   │   ├── IdempotencyMiddleware.cs       # Idempotency key enforcement
│   │   └── ExceptionHandlingMiddleware.cs
│   └── Program.cs
│
└── tests/
    ├── BitcoinPayments.UnitTests/
    │   ├── Domain/
    │   ├── Application/
    │   └── Infrastructure/
    ├── BitcoinPayments.IntegrationTests/
    │   ├── Bitcoin/                        # Tests against regtest/testnet
    │   └── Persistence/                    # Tests against real PostgreSQL
    └── BitcoinPayments.EndToEndTests/
        └── PaymentFlowTests.cs            # Full charge, auth→capture, auth→void, refund flows
```

### 3.2 Key Technology Decisions

| Concern | Choice | Rationale |
|---------|--------|-----------|
| Bitcoin library | NBitcoin (latest stable) | De facto standard for .NET Bitcoin development. Maintained by Nicolas Dorier. Covers HD wallets, transaction building, script construction, PSBT support. |
| ORM | EF Core 8+ with Npgsql | Standard .NET data access, excellent PostgreSQL support. |
| Database | PostgreSQL | Robust, well-supported, handles JSON columns for flexible tx metadata. |
| Validation | FluentValidation | Declarative validation for command objects. |
| Mediator | MediatR | Command/query separation in the application layer. |
| Logging | Serilog | Structured logging with correlation IDs for transaction tracing. |
| Testing | xUnit + Moq + Testcontainers | Testcontainers for PostgreSQL integration tests. |
| API docs | Swagger / Swashbuckle | Auto-generated API documentation. |
| Background jobs | .NET BackgroundService | Confirmation watching, auth expiry monitoring. |

### 3.3 Mainnet Safety Guard

This is a critical safety mechanism. Implement a startup guard that prevents the application from running if any configuration points to mainnet.

```csharp
// This must run at application startup before any Bitcoin operations
public class NetworkGuard
{
    public static void EnsureTestnet(BitcoinOptions options)
    {
        if (options.Network == "mainnet" || options.Network == "main")
            throw new InvalidOperationException(
                "FATAL: This application is configured for TESTNET ONLY. " +
                "Mainnet operation is not permitted.");

        // Double-check: ensure NBitcoin Network object is testnet
        var network = Network.GetNetwork(options.Network);
        if (network == Network.Main)
            throw new InvalidOperationException(
                "FATAL: Resolved network is mainnet. Aborting.");
    }
}
```

Register this in `Program.cs` so it runs before the host starts.

---

## 4. Bitcoin Test Environment — Development and QA

### 4.1 Testnet4 Overview

The initial deployment environment uses **Bitcoin Testnet4** (`Network.TestNet4` in NBitcoin). Testnet4 is the current recommended test network — it uses worthless test coins, has the same scripting rules as mainnet, and is publicly accessible.

**Key properties**:
- Coins are free (obtained from faucets).
- Block times are irregular (can be faster or slower than mainnet's ~10 min average).
- Mining difficulty is low.
- Address prefix: `tb1` (bech32) or `m`/`n` (legacy).

### 4.2 Setting Up Test Wallets

NBitcoin handles all wallet operations natively. No external wallet software is needed.

**HD Wallet Generation (BIP32/BIP44)**:
```csharp
// Generate a new HD wallet
var mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
// STORE THE MNEMONIC SECURELY — it is the master seed

var masterKey = mnemonic.DeriveExtKey();  // BIP32 master key
var network = Network.TestNet4;

// BIP44 derivation path for Bitcoin testnet: m/44'/1'/0'
// Account 0, external chain (receiving addresses)
var accountKey = masterKey.Derive(new KeyPath("m/44'/1'/0'"));
var receivingKey0 = accountKey.Derive(0).Derive(0);  // First receiving address
var receivingKey1 = accountKey.Derive(0).Derive(1);  // Second receiving address

// Change addresses: m/44'/1'/0'/1/index
var changeKey0 = accountKey.Derive(1).Derive(0);

var address = receivingKey0.GetPublicKey().GetAddress(ScriptPubKeyType.Segwit, network);
// Result: tb1q... address
```

**For the deployment, create these wallets at startup (or via seed endpoint)**:
- **Buyer Wallet** — represents the customer making a payment.
- **Merchant Wallet** — represents the merchant receiving payment.
- **Platform Wallet** — the platform's operational wallet (co-signer for escrow operations).

Each wallet must persist its extended private key (encrypted at rest) and track its derivation index so addresses are never reused.

### 4.3 Funding Test Wallets (Testnet Faucets)

Test wallets need testnet coins to transact. Obtain coins from testnet faucets:

- **mempool.space Testnet4 Faucet**: https://mempool.space/testnet4/faucet
- **bitcoinfaucet.uo1.net**: https://bitcoinfaucet.uo1.net/ (supports testnet4)
- **Alternative**: Use `bitcoin-cli` to mine blocks on a local regtest network for fully offline testing (see Section 4.5).

**Implement a dev-only seed endpoint** in the API for operational convenience:
```
POST /api/dev/fund-wallet
{
    "walletId": "buyer-wallet-id",
    "amountBtc": 0.01
}
```
This endpoint must only be available in non-production environments. It can either provide instructions/links to manually use a faucet with the wallet's current receiving address, or if running in regtest mode, automatically generate blocks to fund the wallet.

### 4.4 Testnet Transaction Monitoring

Use these block explorers to verify transactions on testnet:

- **mempool.space Testnet4**: `https://mempool.space/testnet4/tx/{txid}`
- **blockstream.info Testnet**: `https://blockstream.info/testnet/tx/{txid}`

The `BlockchainMonitor` background service should use one of these APIs (or a self-hosted Electrum server) to:
- Check transaction confirmation count.
- Detect when a transaction enters the mempool.
- Retrieve UTXO set for wallet addresses.

**Recommended API**: mempool.space has a public REST API:
- `GET /api/tx/{txid}` — transaction details
- `GET /api/address/{address}/utxo` — UTXOs for an address
- `GET /api/tx/{txid}/status` — confirmation status
- `POST /api/tx` — broadcast raw transaction hex

### 4.5 Regtest Mode (Automated Testing / CI)

For automated tests and CI pipelines, use **regtest** (regression test mode) instead of public testnet. Regtest is a private blockchain you control locally.

**Setup with Docker**:
```yaml
# docker-compose.yml (test environment)
services:
  bitcoind-regtest:
    image: kylemanna/bitcoind
    command: >
      -regtest
      -server
      -rpcuser=testuser
      -rpcpassword=testpass
      -rpcallowip=0.0.0.0/0
      -rpcbind=0.0.0.0
      -txindex=1
      -fallbackfee=0.00001
    ports:
      - "18443:18443"  # regtest RPC port

  postgres:
    image: postgres:16
    environment:
      POSTGRES_DB: btcpayments
      POSTGRES_USER: app
      POSTGRES_PASSWORD: testpass
    ports:
      - "5432:5432"
```

**Regtest advantages**:
- Instant block generation: `bitcoin-cli -regtest generatetoaddress 1 <address>` creates a block immediately.
- No faucets needed — mine coins directly to your test wallets.
- Deterministic — tests are reproducible.
- Fully offline — no network dependency.

**NBitcoin regtest configuration**:
```csharp
var network = Network.RegTest;
// Connect to local bitcoind RPC
var rpcClient = new RPCClient("testuser:testpass", "localhost:18443", network);

// Mine 101 blocks to a test address (coinbase maturity = 100 blocks)
var minerAddress = new Key().GetAddress(ScriptPubKeyType.Segwit, network);
await rpcClient.GenerateToAddressAsync(101, minerAddress);
```

### 4.6 Testing Strategy by Payment Operation

#### Charge (Sale) — Test Scenarios

| # | Scenario | Setup | Verification |
|---|----------|-------|-------------|
| 1 | Successful charge | Fund buyer wallet via faucet/regtest → execute charge → mine block | Verify: tx confirmed, merchant UTXO exists, buyer balance decreased, state = Settled |
| 2 | Charge with insufficient funds | Buyer wallet empty or below amount + fees | Verify: operation fails with clear error, no tx broadcast, state = Failed |
| 3 | Charge idempotency | Submit same charge request (same idempotency key) twice | Verify: second call returns original response, no duplicate tx |
| 4 | Charge fee estimation | Execute charge with different fee rate configurations | Verify: transaction size and fee are reasonable, change output exists |

#### Auth → Capture Flow — Test Scenarios

| # | Scenario | Setup | Verification |
|---|----------|-------|-------------|
| 1 | Happy path auth + capture | Create auth → verify escrow funded → capture → mine block | Verify: escrow UTXO spent, merchant received funds, state transitions correct |
| 2 | Partial capture | Auth for 0.01 BTC → capture 0.006 BTC | Verify: merchant gets 0.006, buyer gets change (minus fees), escrow fully spent |
| 3 | Auth expiry | Create auth → wait for timelock to elapse (regtest: mine N blocks) | Verify: auto-void triggered, buyer funds returned, state = Expired |
| 4 | Double capture prevention | Auth → capture → attempt second capture | Verify: second capture rejected, escrow already spent |

#### Void — Test Scenarios

| # | Scenario | Setup | Verification |
|---|----------|-------|-------------|
| 1 | Void before confirmation | Create auth (funding tx unconfirmed) → void via RBF | Verify: original funding tx replaced, buyer refunded, state = Voided |
| 2 | Cooperative void after confirmation | Create auth → mine block (confirm funding) → void | Verify: both parties sign refund from escrow, buyer receives funds |
| 3 | Void after capture | Auth → capture → attempt void | Verify: void rejected (already captured) |

#### Refund — Test Scenarios

| # | Scenario | Setup | Verification |
|---|----------|-------|-------------|
| 1 | Full refund after charge | Charge → settle → refund full amount | Verify: new tx from merchant to buyer, refund amount correct minus fees |
| 2 | Partial refund | Charge 0.01 BTC → refund 0.004 BTC | Verify: buyer receives 0.004, merchant retains remainder |
| 3 | Refund exceeds original | Attempt refund greater than original charge | Verify: rejected with validation error |

---

## 5. Database Schema

### 5.1 Core Tables

```sql
-- Wallets table — HD wallet metadata
CREATE TABLE wallets (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(100) NOT NULL,                        -- e.g., "buyer", "merchant", "platform"
    encrypted_master_key TEXT NOT NULL,                 -- encrypted BIP32 extended private key
    current_receiving_index INT NOT NULL DEFAULT 0,     -- next BIP44 receiving address index
    current_change_index INT NOT NULL DEFAULT 0,        -- next BIP44 change address index
    network VARCHAR(20) NOT NULL DEFAULT 'testnet4',    -- testnet4, regtest
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Tracked UTXOs
CREATE TABLE utxos (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    wallet_id UUID NOT NULL REFERENCES wallets(id),
    transaction_id VARCHAR(64) NOT NULL,                -- txid hex
    output_index INT NOT NULL,                          -- vout
    amount_satoshis BIGINT NOT NULL,
    script_pubkey TEXT NOT NULL,                         -- hex-encoded scriptPubKey
    address VARCHAR(100) NOT NULL,
    is_spent BOOLEAN NOT NULL DEFAULT FALSE,
    spent_by_tx VARCHAR(64),                            -- txid that spent this UTXO
    confirmation_count INT NOT NULL DEFAULT 0,
    is_escrow BOOLEAN NOT NULL DEFAULT FALSE,            -- true if this is an escrow UTXO
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE(transaction_id, output_index)
);

CREATE INDEX idx_utxos_wallet_unspent ON utxos(wallet_id, is_spent) WHERE NOT is_spent;

-- Payment transactions — aggregate root
CREATE TABLE payment_transactions (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    idempotency_key VARCHAR(100) UNIQUE,                -- client-provided idempotency key
    operation_type VARCHAR(20) NOT NULL,                 -- charge, auth, capture, void, refund
    state VARCHAR(30) NOT NULL,                          -- see state machine in Section 2
    amount_satoshis BIGINT NOT NULL,                     -- requested amount
    fee_satoshis BIGINT,                                 -- actual fee paid
    buyer_wallet_id UUID NOT NULL REFERENCES wallets(id),
    merchant_wallet_id UUID NOT NULL REFERENCES wallets(id),
    bitcoin_tx_id VARCHAR(64),                           -- on-chain txid once broadcast
    raw_transaction_hex TEXT,                             -- signed tx hex for rebroadcast
    confirmation_count INT NOT NULL DEFAULT 0,
    parent_transaction_id UUID REFERENCES payment_transactions(id),  -- links capture→auth, refund→charge
    metadata JSONB,                                      -- flexible storage for operation-specific data
    error_message TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_payment_tx_state ON payment_transactions(state);
CREATE INDEX idx_payment_tx_bitcoin_txid ON payment_transactions(bitcoin_tx_id);
CREATE INDEX idx_payment_tx_parent ON payment_transactions(parent_transaction_id);

-- Escrow records — for auth/capture/void flows
CREATE TABLE escrows (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    payment_transaction_id UUID NOT NULL REFERENCES payment_transactions(id),
    escrow_address VARCHAR(100) NOT NULL,                -- P2SH or P2WSH escrow address
    redeem_script_hex TEXT NOT NULL,                      -- the multisig redeem script
    buyer_pubkey_hex VARCHAR(66) NOT NULL,
    merchant_pubkey_hex VARCHAR(66) NOT NULL,
    timelock_blocks INT NOT NULL,                         -- CLTV block height for auto-refund
    timelock_refund_tx_hex TEXT,                           -- pre-signed timelock refund tx
    funding_utxo_id UUID REFERENCES utxos(id),            -- the UTXO in the escrow address
    state VARCHAR(30) NOT NULL,                           -- funded, captured, voided, expired
    expires_at_block INT,                                  -- block height at which auth expires
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Idempotency key store
CREATE TABLE idempotency_keys (
    key VARCHAR(100) PRIMARY KEY,
    response_status_code INT NOT NULL,
    response_body TEXT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    expires_at TIMESTAMPTZ NOT NULL DEFAULT (NOW() + INTERVAL '24 hours')
);

CREATE INDEX idx_idempotency_expires ON idempotency_keys(expires_at);
```

---

## 6. API Design

### 6.1 Endpoints

```
POST   /api/v1/payments/charge          # Execute a direct charge (sale)
POST   /api/v1/payments/authorize        # Create an authorization hold
POST   /api/v1/payments/capture          # Capture a previous authorization
POST   /api/v1/payments/void             # Void a previous authorization
POST   /api/v1/payments/refund           # Refund a previous charge or capture
GET    /api/v1/payments/{id}             # Get payment transaction status
GET    /api/v1/payments/{id}/history     # Get full state transition history

POST   /api/v1/wallets                   # Create a new wallet
GET    /api/v1/wallets/{id}              # Get wallet info and balance
GET    /api/v1/wallets/{id}/address      # Generate next receiving address
GET    /api/v1/wallets/{id}/utxos        # List unspent outputs
```

### 6.2 Request/Response Examples

**Charge Request**:
```json
POST /api/v1/payments/charge
Idempotency-Key: "chg_abc123"
{
    "buyerWalletId": "uuid",
    "merchantWalletId": "uuid",
    "amountSatoshis": 100000,
    "feeRateSatPerByte": 2,
    "metadata": {
        "orderId": "ORD-2024-001",
        "description": "Widget purchase"
    }
}
```

**Charge Response**:
```json
{
    "id": "uuid",
    "operationType": "charge",
    "state": "mempool",
    "amountSatoshis": 100000,
    "feeSatoshis": 450,
    "bitcoinTxId": "a1b2c3d4...",
    "confirmationCount": 0,
    "explorerUrl": "https://mempool.space/testnet4/tx/a1b2c3d4...",
    "createdAt": "2025-01-15T10:30:00Z"
}
```

**Authorization Request**:
```json
POST /api/v1/payments/authorize
Idempotency-Key: "auth_xyz789"
{
    "buyerWalletId": "uuid",
    "merchantWalletId": "uuid",
    "amountSatoshis": 500000,
    "authWindowBlocks": 144,
    "metadata": {
        "orderId": "ORD-2024-002"
    }
}
```

**Capture Request**:
```json
POST /api/v1/payments/capture
Idempotency-Key: "cap_def456"
{
    "authorizationId": "uuid-of-original-auth",
    "amountSatoshis": 400000,
    "metadata": {
        "captureReason": "partial_shipment"
    }
}
```

### 6.3 Idempotency

Every mutating endpoint requires an `Idempotency-Key` header. The middleware must:
1. Check if a transaction with this key already exists.
2. If yes, return the existing result (same HTTP status, same body).
3. If no, proceed with processing and store the key with the result.
4. Keys expire after 24 hours.

---

## 7. Implementation Priorities

Build in this order. Each phase should be fully tested before moving to the next.

### Phase 1 — Foundation
- [ ] Solution structure with all projects and references.
- [ ] PostgreSQL schema + EF Core DbContext + migrations.
- [ ] Wallet management: HD wallet creation, key derivation, address generation via NBitcoin.
- [ ] Mainnet safety guard.
- [ ] Basic API scaffolding with Swagger.
- [ ] Docker Compose for local development (PostgreSQL + bitcoind regtest).

### Phase 2 — Charge (Sale)
- [ ] UTXO tracking service (fetch and persist UTXOs for wallet addresses).
- [ ] Coin selection algorithm (start with largest-first, optimize later).
- [ ] Transaction building + signing via NBitcoin `TransactionBuilder`.
- [ ] Transaction broadcasting to testnet / regtest.
- [ ] Confirmation watcher background service.
- [ ] Charge endpoint end-to-end.
- [ ] Unit + integration tests for charge flow.

### Phase 3 — Authorization + Capture
- [ ] Escrow script construction (2-of-2 multisig + CLTV timelock).
- [ ] Timelock refund pre-signing.
- [ ] Auth flow: fund escrow → track escrow UTXO.
- [ ] Capture flow: multisig spend from escrow → merchant.
- [ ] Partial capture support.
- [ ] Auth expiry monitor background service.
- [ ] Unit + integration tests for auth/capture flows.

### Phase 4 — Void + Refund
- [ ] Void: cooperative close (both-party sign returning to buyer).
- [ ] Void: RBF path for unconfirmed funding tx.
- [ ] Refund: new transaction from merchant to buyer.
- [ ] Partial refund support.
- [ ] Validation: prevent void after capture, prevent refund exceeding original, etc.
- [ ] Unit + integration tests for void/refund flows.

### Phase 5 — Hardening
- [ ] Idempotency middleware.
- [ ] Comprehensive error handling and domain exceptions.
- [ ] Structured logging with Serilog (correlation IDs on every tx).
- [ ] End-to-end flow tests (full lifecycle scenarios).
- [ ] API documentation polish.
- [ ] Health check endpoints.

---

## 8. NBitcoin Quick Reference

### 8.1 Essential Classes

| Class | Purpose |
|-------|---------|
| `Mnemonic` | BIP39 mnemonic generation and seed derivation |
| `ExtKey` / `ExtPubKey` | BIP32 hierarchical deterministic key derivation |
| `Key` / `PubKey` | Individual private/public key pairs |
| `BitcoinAddress` | Address encoding/decoding (all formats) |
| `Transaction` | Raw Bitcoin transaction (build, sign, serialize) |
| `TransactionBuilder` | High-level transaction construction with coin selection |
| `Coin` / `ScriptCoin` | UTXO representation for transaction inputs |
| `Script` | Bitcoin Script construction and parsing |
| `PayToMultiSigTemplate` | Helper for multisig script generation |
| `PSBT` | Partially Signed Bitcoin Transaction (BIP174) |
| `Network` | Network parameters (TestNet, TestNet4, RegTest, Main) |
| `RPCClient` | JSON-RPC client for bitcoind |
| `Op` | Individual script opcodes (OP_CHECKMULTISIG, OP_CHECKLOCKTIMEVERIFY, etc.) |

### 8.2 Common Patterns

**Build and broadcast a simple transaction**:
```csharp
var coins = GetUnspentCoins(buyerWallet);  // List<Coin>
var txBuilder = network.CreateTransactionBuilder();
var tx = txBuilder
    .AddCoins(coins)
    .AddKeys(buyerPrivateKey)
    .Send(merchantAddress, Money.Satoshis(amountSatoshis))
    .SetChange(buyerChangeAddress)
    .SendEstimatedFees(new FeeRate(Money.Satoshis(feeRateSatPerByte)))
    .BuildTransaction(sign: true);

// Verify before broadcast
var verified = txBuilder.Verify(tx);

// Broadcast
var rawHex = tx.ToHex();
// Send rawHex to mempool.space API or bitcoind RPC
```

**Create a 2-of-2 multisig escrow address**:
```csharp
var buyerPubKey = buyerKey.PubKey;
var merchantPubKey = merchantKey.PubKey;

var redeemScript = PayToMultiSigTemplate.Instance.GenerateScriptPubKey(
    sigCount: 2,
    pubkeys: new[] { buyerPubKey, merchantPubKey }
);

var escrowAddress = redeemScript.WitHash.GetAddress(network);  // P2WSH address
// Store redeemScript — needed to spend from this address later
```

**Spend from multisig escrow (capture)**:
```csharp
var escrowCoin = new Coin(fundingTxId, fundingOutputIndex, escrowAmount, redeemScript.WitHash.ScriptPubKey);
var scriptCoin = escrowCoin.ToScriptCoin(redeemScript);

var captureTx = network.CreateTransactionBuilder()
    .AddCoins(scriptCoin)
    .AddKeys(buyerKey, merchantKey)
    .Send(merchantAddress, captureAmount)
    .SetChange(buyerChangeAddress)    // for partial captures
    .SendEstimatedFees(feeRate)
    .BuildTransaction(sign: true);
```

**Timelock refund script (CLTV)**:
```csharp
// Combined script: multisig path + timelock refund path
var refundScript = new Script(
    OpcodeType.OP_IF,
        // Path 1: Normal multisig (auth is active)
        Op.GetPushOp(2),
        Op.GetPushOp(buyerPubKey.ToBytes()),
        Op.GetPushOp(merchantPubKey.ToBytes()),
        Op.GetPushOp(2),
        OpcodeType.OP_CHECKMULTISIG,
    OpcodeType.OP_ELSE,
        // Path 2: Timelock refund (auth expired)
        Op.GetPushOp(lockBlockHeight),
        OpcodeType.OP_CHECKLOCKTIMEVERIFY,
        OpcodeType.OP_DROP,
        Op.GetPushOp(buyerPubKey.ToBytes()),
        OpcodeType.OP_CHECKSIG,
    OpcodeType.OP_ENDIF
);
```

---

## 9. Configuration

### 9.1 appsettings.json

```json
{
    "Bitcoin": {
        "Network": "testnet4",
        "DefaultFeeRateSatPerByte": 2,
        "AuthWindowBlocks": 144,
        "MinConfirmationsForSettlement": 3,
        "MaxConfirmationsToTrack": 6,
        "MempoolApiBaseUrl": "https://mempool.space/testnet4/api",
        "AllowMainnet": false
    },
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=btcpayments;Username=app;Password=..."
    },
    "BackgroundServices": {
        "ConfirmationWatcherIntervalSeconds": 30,
        "AuthExpiryCheckIntervalSeconds": 60
    }
}
```

### 9.2 Environment-Specific Overrides

- **Development**: Use testnet4 with faucets, enable dev endpoints (`/api/dev/*`), verbose logging.
- **Test/CI**: Use regtest with dockerized bitcoind, Testcontainers for PostgreSQL.
- **Staging**: Testnet4 with production-like configuration (no dev endpoints).
- **Production**: Testnet4 initially. Mainnet support is a future milestone gated behind `AllowMainnet` flag, feature flags, and security audit.

---

## 10. Error Handling

### 10.1 Domain Exceptions

Define specific exception types for clear error communication:

```csharp
public class InsufficientFundsException : DomainException { }
public class EscrowNotFoundException : DomainException { }
public class InvalidOperationStateException : DomainException { }   // e.g., void after capture
public class AuthorizationExpiredException : DomainException { }
public class TransactionBroadcastException : DomainException { }
public class DuplicateOperationException : DomainException { }      // idempotency violation
public class MainnetGuardException : DomainException { }            // blocked mainnet attempt
```

### 10.2 API Error Response Format

```json
{
    "error": {
        "code": "INSUFFICIENT_FUNDS",
        "message": "Buyer wallet has 50,000 satoshis available but 100,000 required (including estimated fee of 450 satoshis).",
        "details": {
            "availableSatoshis": 50000,
            "requestedSatoshis": 100000,
            "estimatedFeeSatoshis": 450
        }
    }
}
```

---

## 11. Coding Standards

- All public methods and interfaces must have XML documentation comments.
- Use `CancellationToken` on all async methods.
- Use `record` types for DTOs, commands, and value objects where appropriate.
- No `string` for Bitcoin addresses or transaction IDs in domain/application layers — use strongly-typed value objects.
- Log every state transition with structured context (transaction ID, operation type, old state, new state).
- Use guard clauses at the top of methods — fail fast, fail loud.
- No magic numbers — all Bitcoin-specific constants (dust limit, max block size, etc.) should be named constants.
- All monetary amounts are in satoshis (long/Int64) internally. BTC display formatting is an API/presentation concern only.
- Follow the repository pattern — domain layer defines interfaces, infrastructure implements them.
- All external API calls (mempool.space, bitcoind RPC) must have retry policies with exponential backoff (Polly).
- Sensitive data (private keys, mnemonics) must never appear in logs.

---

## 12. Glossary

| Term | Definition |
|------|-----------|
| **UTXO** | Unspent Transaction Output — the fundamental unit of Bitcoin value. You don't have a "balance"; you have a set of UTXOs. |
| **Satoshi** | The smallest unit of Bitcoin. 1 BTC = 100,000,000 satoshis. All internal amounts use satoshis. |
| **P2SH** | Pay-to-Script-Hash — an address type where spending requires satisfying a script (used for multisig escrow). |
| **P2WSH** | Pay-to-Witness-Script-Hash — SegWit version of P2SH (preferred, lower fees). |
| **Multisig (M-of-N)** | A script requiring M signatures from N possible keys. We use 2-of-2 for escrow. |
| **CLTV** | CheckLockTimeVerify — an opcode that makes a UTXO unspendable until a specified block height. Used for auth expiry. |
| **HTLC** | Hash Time-Locked Contract — a script that can be spent with a secret (hash preimage) or refunded after a timelock. |
| **RBF** | Replace-By-Fee — a protocol feature allowing an unconfirmed transaction to be replaced with a higher-fee version. Used for void-before-confirmation. |
| **PSBT** | Partially Signed Bitcoin Transaction — a format for passing unsigned/partially-signed transactions between parties. |
| **BIP32** | Hierarchical Deterministic wallets — derive unlimited keys from a single master seed. |
| **BIP44** | Multi-account hierarchy for BIP32 — defines standard derivation paths (m/44'/coin'/account'/change/index). |
| **Regtest** | Regression test mode — a private, locally-controlled blockchain for automated testing. |
| **Testnet4** | Bitcoin's public test network (4th iteration). Free coins, same rules as mainnet. |
| **Mempool** | The set of unconfirmed transactions waiting to be included in a block. |
| **Coin Selection** | The algorithm for choosing which UTXOs to spend in a transaction (minimize fees, avoid dust, etc.). |
| **Dust Limit** | The minimum output value below which Bitcoin nodes will reject a transaction (~546 satoshis for standard outputs). |
| **HD Wallet** | Hierarchical Deterministic wallet — generates a tree of key pairs from a single seed (BIP32). |
| **Escrow** | A conditional holding arrangement where funds are locked until predefined conditions are met (multisig + timelock). |
