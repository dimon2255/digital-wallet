# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Bitcoin payment processing platform ("ChainVault") built with .NET 10 and Clean Architecture. Supports charge, authorize/capture/void, refund, and wallet-to-wallet transfer flows against Bitcoin testnet4/regtest. Includes ASP.NET Core Identity + JWT authentication, user-scoped wallets, and a React SPA frontend. **Mainnet is permanently disallowed** via `NetworkGuard`.

## Build & Run

```bash
# Build
dotnet build BitcoinPayments.sln

# Run API (default: https://localhost:5001, http://localhost:5000)
dotnet run --project src/BitcoinPayments.API

# Run SPA dev server (proxies API to localhost:5001)
cd src/BitcoinPayments.UI && npm run dev

# Build SPA for production (outputs to src/BitcoinPayments.API/wwwroot/)
cd src/BitcoinPayments.UI && npm run build

# Infrastructure (PostgreSQL + bitcoind regtest)
docker-compose up -d

# EF Core migrations (tool: dotnet-ef 10.x via .config/dotnet-tools.json)
dotnet tool restore
dotnet ef migrations add <Name> --project src/BitcoinPayments.Infrastructure --startup-project src/BitcoinPayments.API
dotnet ef database update --project src/BitcoinPayments.Infrastructure --startup-project src/BitcoinPayments.API
```

## Tests

```bash
# All tests
dotnet test BitcoinPayments.sln

# Single project
dotnet test tests/BitcoinPayments.UnitTests
dotnet test tests/BitcoinPayments.IntegrationTests

# Single test
dotnet test tests/BitcoinPayments.UnitTests --filter "FullyQualifiedName~CoinSelectionServiceTests"

# E2E tests require docker-compose services running (skipped by default)
dotnet test tests/BitcoinPayments.EndToEndTests
```

- **UnitTests**: xunit + Moq. Tests Application and Infrastructure logic in isolation.
- **IntegrationTests**: xunit + Testcontainers.PostgreSql. Tests EF Core persistence against real Postgres.
- **EndToEndTests**: xunit + `Microsoft.AspNetCore.Mvc.Testing`. Tests full HTTP flows; requires docker-compose services.

## Architecture

Clean Architecture with four layers. Dependency flows inward: API -> Application -> Domain; Infrastructure implements Domain interfaces.

### Domain (`BitcoinPayments.Domain`)
- **Entities**: `Wallet` (with `UserId`), `PaymentTransaction`, `Utxo`, `Escrow`, `IdempotencyKeyRecord`, `ApplicationUser` (extends IdentityUser)
- **Value Objects**: `Money` (satoshis), `BitcoinAddress`, `TransactionId`, `PaymentOperationType` (incl. Transfer=6)
- **Enums**: `TransactionState` (incl. TransferCompleted=16), `EscrowState`, `WalletAddressPurpose`
- **Interfaces**: Repository contracts (`IWalletRepository`, `ITransactionRepository`, `IUtxoRepository`, `IEscrowRepository`, `IIdempotencyKeyRepository`, `IBitcoinNetwork`)
- Depends on NBitcoin + Microsoft.Extensions.Identity.Stores

### Application (`BitcoinPayments.Application`)
- **Commands + Handlers** via MediatR: `CreateChargeCommand` -> `ChargeHandler`, `CreateAuthCommand` -> `AuthHandler`, `CaptureAuthCommand` -> `CaptureHandler`, `VoidAuthCommand` -> `VoidHandler`, `CreateRefundCommand` -> `RefundHandler`, `CreateTransferCommand` -> `TransferHandler`
- **Services**: `CoinSelectionService`, `FeeEstimationService`, `TransactionBuilderService`, `EscrowService`, `WalletSynchronizationService`, `WalletApplicationService` (user-scoped), `PaymentQueryService` (paginated)
- **Transfer System**: `ITransferStrategy`, `InternalLedgerStrategy`, `OnChainTransferStrategy`, `TransferStrategyResolver` (auto/onchain/internal modes)
- **Validation**: FluentValidation with `ValidationBehavior<,>` MediatR pipeline behavior
- DI registration: `services.AddApplication()`

### Infrastructure (`BitcoinPayments.Infrastructure`)
- **Persistence**: EF Core + Npgsql (PostgreSQL). `AppDbContext` extends `IdentityDbContext<ApplicationUser>`. Design-time factory: `AppDbContextFactory`
- **Repository implementations** in `Persistence/Repositories/`
- **Identity**: `JwtTokenService` (access + refresh tokens), `JwtOptions`, `RefreshToken` entity, `DemoSeedService` (seeds demo@chainvault.dev)
- **Bitcoin**: `NBitcoinNetworkService` (network interaction via mempool.space API), `HdWalletManager` (BIP32 HD key derivation with DataProtection encryption), `ScriptBuilder`, `TestnetBroadcaster`
- **Background services**: `ConfirmationWatcher` (polls confirmation counts), `AuthExpiryMonitor` (expires stale authorizations)
- **Config**: `BitcoinOptions` (incl. TransferMode), `BackgroundServiceOptions`, `NetworkGuard` (hard blocks mainnet)
- HTTP client with Polly retry for mempool.space API
- DI registration: `services.AddInfrastructure(configuration)`

### API (`BitcoinPayments.API`)
- Controllers: `AccountsController` (register/login/refresh/demo-login), `PaymentsController` (`api/v1/payments/*`), `WalletsController` (user-scoped), `TransactionsController` (paginated), `TransfersController`, `DevController`
- Middleware: `ExceptionHandlingMiddleware` (incl. 401/403), `IdempotencyMiddleware` (skips /accounts routes)
- Authentication: JWT Bearer + CORS for SPA on localhost:5173
- Serves React SPA via static files + MapFallbackToFile
- Scalar API docs enabled in Development; health check at `/health`
- Auto-runs EF migrations + demo seed on startup
- Logging: Serilog

### Frontend (`BitcoinPayments.UI`)
- React 19 + TypeScript + Vite + Tailwind CSS v4
- State: Zustand (auth), TanStack Query (server state)
- Routing: React Router v7
- Pages: Login (with "Try Demo"), Dashboard, Wallets, Wallet Detail (QR code), Send/Transfer, Transaction History (paginated), Settings
- Dark theme: zinc-950 base, amber/orange Bitcoin accents, "ChainVault" branding

## Key Design Decisions

- All payment operations require an `Idempotency-Key` header (except /accounts endpoints)
- JWT auth: 15min access tokens, 7-day refresh tokens (dev: 60min/30d)
- Wallets are user-scoped via `Wallet.UserId` — ownership enforced in service layer
- Transfer system: configurable via `Bitcoin.TransferMode` (auto/onchain/internal). Auto = internal when same user, on-chain otherwise
- Demo user (demo@chainvault.dev / Demo123!) seeded on startup with 3 wallets + sample transactions
- Authorize flow uses on-chain escrow with timelock scripts (not custodial holding)
- HD wallets use BIP32 derivation; master keys are encrypted via ASP.NET DataProtection
- UTXO tracking is internal (wallet sync pulls from mempool.space API)
- `NetworkGuard` throws `MainnetGuardException` at startup if network resolves to mainnet — never bypass this

## Configuration

- `Jwt` section in appsettings: Secret, Issuer, Audience, token lifetimes
- `Bitcoin` section in appsettings: network, fee rates, confirmation thresholds, RPC credentials, mempool API URL, TransferMode
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection string
- `BackgroundServices`: polling intervals for confirmation watcher and auth expiry monitor
- Development defaults to `testnet4` network with relaxed auth windows
