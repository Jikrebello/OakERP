using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OakERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArInvoiceLineLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "location_id",
                table: "ar_invoice_lines",
                type: "uuid",
                nullable: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_ar_invoice_lines_location_id",
                table: "ar_invoice_lines",
                column: "location_id"
            );

            migrationBuilder.AddForeignKey(
                name: "fk_ar_invoice_lines_locations_location_id",
                table: "ar_invoice_lines",
                column: "location_id",
                principalTable: "locations",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_ar_invoice_lines_locations_location_id",
                table: "ar_invoice_lines"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_ledgers_items_item_id",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropForeignKey(
                name: "fk_inventory_ledgers_locations_location_id",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropIndex(
                name: "ix_inventory_ledgers_item_id_location_id_trx_date",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropIndex(
                name: "ix_inventory_ledgers_item_id_trx_date",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropIndex(
                name: "ix_inventory_ledgers_source_type_source_id",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropIndex(
                name: "ix_inventory_ledgers_transaction_type",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_invledg_qty_nonzero",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_invledg_source_pairing",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_invledg_unitcost_nonneg",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropCheckConstraint(
                name: "ck_invledg_valuechange_sign",
                table: "inventory_ledgers"
            );

            migrationBuilder.DropIndex(
                name: "ix_ar_invoice_lines_location_id",
                table: "ar_invoice_lines"
            );

            migrationBuilder.DropColumn(name: "location_id", table: "ar_invoice_lines");
        }
    }
}
