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

            migrationBuilder.DropIndex(
                name: "ix_ar_invoice_lines_location_id",
                table: "ar_invoice_lines"
            );

            migrationBuilder.DropColumn(name: "location_id", table: "ar_invoice_lines");
        }
    }
}
