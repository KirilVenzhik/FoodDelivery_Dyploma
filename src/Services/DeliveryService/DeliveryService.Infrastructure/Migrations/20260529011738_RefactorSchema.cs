using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeliveryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "processed_events",
                newName: "processed_at");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "processed_events",
                newName: "event_id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "deliveries",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "deliveries",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "deliveries",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "deliveries",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "deliveries",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "CourierId",
                table: "deliveries",
                newName: "courier_id");

            migrationBuilder.AddColumn<Guid>(
                name: "client_id",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "correlation_id",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "restaurant_id",
                table: "deliveries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "client_id",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "deliveries");

            migrationBuilder.DropColumn(
                name: "restaurant_id",
                table: "deliveries");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "processed_events",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "event_id",
                table: "processed_events",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "deliveries",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "deliveries",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "deliveries",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "deliveries",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "deliveries",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "courier_id",
                table: "deliveries",
                newName: "CourierId");
        }
    }
}
