using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_processed_events",
                table: "processed_events");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "processed_events",
                newName: "processed_at");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "processed_events",
                newName: "correlation_id");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "notifications",
                newName: "type");

            migrationBuilder.RenameColumn(
                name: "Message",
                table: "notifications",
                newName: "message");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "notifications",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "notifications",
                newName: "user_id");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "notifications",
                newName: "sent_at");

            migrationBuilder.RenameColumn(
                name: "OrderId",
                table: "notifications",
                newName: "order_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "notifications",
                newName: "created_at");

            migrationBuilder.AddColumn<string>(
                name: "notification_type",
                table: "processed_events",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "type",
                table: "notifications",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "message",
                table: "notifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "sent_at",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "is_read",
                table: "notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_processed_events",
                table: "processed_events",
                columns: new[] { "correlation_id", "notification_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_processed_events",
                table: "processed_events");

            migrationBuilder.DropColumn(
                name: "notification_type",
                table: "processed_events");

            migrationBuilder.DropColumn(
                name: "is_read",
                table: "notifications");

            migrationBuilder.RenameColumn(
                name: "processed_at",
                table: "processed_events",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "correlation_id",
                table: "processed_events",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "type",
                table: "notifications",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "message",
                table: "notifications",
                newName: "Message");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "notifications",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "notifications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "sent_at",
                table: "notifications",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "order_id",
                table: "notifications",
                newName: "OrderId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "notifications",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_processed_events",
                table: "processed_events",
                column: "EventId");
        }
    }
}
