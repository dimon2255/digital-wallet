# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Bitcoin payment processing platform built with .NET 8 and Clean Architecture. Supports charge, authorize/capture/void, and refund flows against Bitcoin testnet4/regtest. **Mainnet is permanently disallowed** via `NetworkGuard`.

## Build & Run

```bash
# Build
dotnet build BitcoinPayments.sln

# Run API (default: https://localhost:5001, http://localhost:5000)
dotnet run --project src/BitcoinPayments.API

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
- **Entities**: `Wallet`, `PaymentTransaction`, `Utxo`, `Escrow`, `IdempotencyKeyRecord`
- **Value Objects**: `Money` (satoshis), `BitcoinAddress`, `TransactionId`, `PaymentOperationType`
- **Enums**: `TransactionState`, `EscrowState`, `WalletAddressPurpose`
- **Interfaces**: Repository contracts (`IWalletRepository`, `ITransactionRepository`, `IUtxoRepository`, `IEscrowRepository`, `IIdempotencyKeyRepository`, `IBitcoinNetwork`)
- Depends only on NBitcoin

### Application (`BitcoinPayments.Application`)
- **Commands + Handlers** via MediatR: `CreateChargeCommand` -> `ChargeHandler`, `CreateAuthCommand` -> `AuthHandler`, `CaptureAuthCommand` -> `CaptureHandler`, `VoidAuthCommand` -> `VoidHandler`, `CreateRefundCommand` -> `RefundHandler`
- **Services**: `CoinSelectionService`, `FeeEstimationService`, `TransactionBuilderService`, `EscrowService`, `WalletSynchronizationService`, `WalletApplicationService`, `PaymentQueryService`
- **Validation**: FluentValidation with `ValidationBehavior<,>` MediatR pipeline behavior
- DI registration: `services.AddApplication()`

### Infrastructure (`BitcoinPayments.Infrastructure`)
- **Persistence**: EF Core + Npgsql (PostgreSQL). `AppDbContext` with Fluent API configurations. Design-time factory: `AppDbContextFactory`
- **Repository implementations** in `Persistence/Repositories/`
- **Bitcoin**: `NBitcoinNetworkService` (network interaction via mempool.space API), `HdWalletManager` (BIP32 HD key derivation with DataProtection encryption), `ScriptBuilder`, `TestnetBroadcaster`
- **Background services**: `ConfirmationWatcher` (polls confirmation counts), `AuthExpiryMonitor` (expires stale authorizations)
- **Config**: `BitcoinOptions`, `BackgroundServiceOptions`, `NetworkGuard` (hard blocks mainnet)
- HTTP client with Polly retry for mempool.space API
- DI registration: `services.AddInfrastructure(configuration)`

### API (`BitcoinPayments.API`)
- Controllers: `PaymentsController` (`api/v1/payments/*`), `WalletsController`, `TransactionsController`, `DevController`
- Middleware: `ExceptionHandlingMiddleware`, `IdempotencyMiddleware`
- Swagger enabled in Development; health check at `/health`
- Auto-runs EF migrations on startup
- Logging: Serilog

## Key Design Decisions

- All payment operations require an `Idempotency-Key` header
- Authorize flow uses on-chain escrow with timelock scripts (not custodial holding)
- HD wallets use BIP32 derivation; master keys are encrypted via ASP.NET DataProtection
- UTXO tracking is internal (wallet sync pulls from mempool.space API)
- `NetworkGuard` throws `MainnetGuardException` at startup if network resolves to mainnet or `AllowMainnet` is true — never bypass this

## Configuration

- `Bitcoin` section in appsettings: network, fee rates, confirmation thresholds, RPC credentials, mempool API URL
- `ConnectionStrings:DefaultConnection`: PostgreSQL connection string
- `BackgroundServices`: polling intervals for confirmation watcher and auth expiry monitor
- Development defaults to `regtest` network; production appsettings uses `testnet4`
