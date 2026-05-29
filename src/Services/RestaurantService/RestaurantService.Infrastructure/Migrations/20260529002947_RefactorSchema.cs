using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestaurantService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems");

            migrationBuilder.RenameTable(
                name: "Restaurants",
                newName: "restaurants");

            migrationBuilder.RenameTable(
                name: "MenuItems",
                newName: "menu_items");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "restaurants",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "restaurants",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "restaurants",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "restaurants",
                newName: "owner_id");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "menu_items",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "menu_items",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "menu_items",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "RestaurantId",
                table: "menu_items",
                newName: "restaurant_id");

            migrationBuilder.RenameColumn(
                name: "IsAvailable",
                table: "menu_items",
                newName: "available");

            migrationBuilder.RenameIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "menu_items",
                newName: "IX_menu_items_restaurant_id");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "restaurants",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "restaurants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "restaurants",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "restaurants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "menu_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "menu_items",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_restaurants",
                table: "restaurants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_menu_items",
                table: "menu_items",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_menu_items_restaurants_restaurant_id",
                table: "menu_items",
                column: "restaurant_id",
                principalTable: "restaurants",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menu_items_restaurants_restaurant_id",
                table: "menu_items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_restaurants",
                table: "restaurants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_menu_items",
                table: "menu_items");

            migrationBuilder.DropColumn(
                name: "address",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "description",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "restaurants");

            migrationBuilder.DropColumn(
                name: "description",
                table: "menu_items");

            migrationBuilder.RenameTable(
                name: "restaurants",
                newName: "Restaurants");

            migrationBuilder.RenameTable(
                name: "menu_items",
                newName: "MenuItems");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Restaurants",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Restaurants",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Restaurants",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                table: "Restaurants",
                newName: "OwnerUserId");

            migrationBuilder.RenameColumn(
                name: "price",
                table: "MenuItems",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "MenuItems",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "MenuItems",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "restaurant_id",
                table: "MenuItems",
                newName: "RestaurantId");

            migrationBuilder.RenameColumn(
                name: "available",
                table: "MenuItems",
                newName: "IsAvailable");

            migrationBuilder.RenameIndex(
                name: "IX_menu_items_restaurant_id",
                table: "MenuItems",
                newName: "IX_MenuItems_RestaurantId");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Restaurants",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "MenuItems",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Restaurants",
                table: "Restaurants",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MenuItems",
                table: "MenuItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MenuItems_Restaurants_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
