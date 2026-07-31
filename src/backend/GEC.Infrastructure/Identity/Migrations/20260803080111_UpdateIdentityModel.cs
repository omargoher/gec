using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GEC.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "RefreshTokens");
        }
    }
}
