using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEC.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnonymousToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "ck_cart_items_quantity_positive",
                table: "cart_items");

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "carts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTime>(
                name: "last_active",
                table: "carts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "last_active",
                table: "carts");

            migrationBuilder.AlterColumn<Guid>(
                name: "customer_id",
                table: "carts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "ck_cart_items_quantity_positive",
                table: "cart_items",
                sql: "\"quantity\" >= 1");
        }
    }
}
