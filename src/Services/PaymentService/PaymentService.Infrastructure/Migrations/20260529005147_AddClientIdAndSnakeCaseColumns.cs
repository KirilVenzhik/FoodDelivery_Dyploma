using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientIdAndSnakeCaseColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "payments",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "payments",
                newName: "processed_at");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "payments",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CorrelationId",
                table: "payments",
                newName: "correlation_id");

            migrationBuilder.AddColumn<Guid>(
                name: "client_id",
                table: "payments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "payments",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "payments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "payments",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "payments",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "payments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "payments",
                newName: "CorrelationId");
        }
    }
}
