using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinPayments.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "escrows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_transaction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    escrow_address = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    redeem_script_hex = table.Column<string>(type: "text", nullable: false),
                    buyer_pubkey_hex = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: false),
                    merchant_pubkey_hex = table.Column<string>(type: "character varying(66)", maxLength: 66, nullable: false),
                    timelock_blocks = table.Column<int>(type: "integer", nullable: false),
                    timelock_refund_tx_hex = table.Column<string>(type: "text", nullable: true),
                    funding_utxo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    buyer_key_index = table.Column<int>(type: "integer", nullable: false),
                    merchant_key_index = table.Column<int>(type: "integer", nullable: false),
                    buyer_change_index = table.Column<int>(type: "integer", nullable: false),
                    expires_at_block = table.Column<long>(type: "bigint", nullable: true),
                    state = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_escrows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                columns: table => new
                {
                    key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    response_status_code = table.Column<int>(type: "integer", nullable: false),
                    response_body = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotency_keys", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    state = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount_satoshis = table.Column<long>(type: "bigint", nullable: false),
                    fee_satoshis = table.Column<long>(type: "bigint", nullable: true),
                    buyer_wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    merchant_wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bitcoin_tx_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    raw_transaction_hex = table.Column<string>(type: "text", nullable: true),
                    confirmation_count = table.Column<int>(type: "integer", nullable: false),
                    parent_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utxos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    output_index = table.Column<int>(type: "integer", nullable: false),
                    amount_satoshis = table.Column<long>(type: "bigint", nullable: false),
                    script_pubkey = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_spent = table.Column<bool>(type: "boolean", nullable: false),
                    spent_by_tx = table.Column<string>(type: "text", nullable: true),
                    confirmation_count = table.Column<int>(type: "integer", nullable: false),
                    is_escrow = table.Column<bool>(type: "boolean", nullable: false),
                    derivation_index = table.Column<int>(type: "integer", nullable: false),
                    is_change_address = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utxos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    encrypted_master_key = table.Column<string>(type: "text", nullable: false),
                    current_receiving_index = table.Column<int>(type: "integer", nullable: false),
                    current_change_index = table.Column<int>(type: "integer", nullable: false),
                    network = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallets", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_idempotency_expires",
                table: "idempotency_keys",
                column: "expires_at");

            migrationBuilder.CreateIndex(
                name: "idx_payment_tx_bitcoin_txid",
                table: "payment_transactions",
                column: "bitcoin_tx_id");

            migrationBuilder.CreateIndex(
                name: "idx_payment_tx_parent",
                table: "payment_transactions",
                column: "parent_transaction_id");

            migrationBuilder.CreateIndex(
                name: "idx_payment_tx_state",
                table: "payment_transactions",
                column: "state");

            migrationBuilder.CreateIndex(
                name: "IX_payment_transactions_idempotency_key",
                table: "payment_transactions",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_utxos_wallet_unspent",
                table: "utxos",
                columns: new[] { "wallet_id", "is_spent" });

            migrationBuilder.CreateIndex(
                name: "IX_utxos_transaction_id_output_index",
                table: "utxos",
                columns: new[] { "transaction_id", "output_index" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "escrows");

            migrationBuilder.DropTable(
                name: "idempotency_keys");

            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "utxos");

            migrationBuilder.DropTable(
                name: "wallets");
        }
    }
}
