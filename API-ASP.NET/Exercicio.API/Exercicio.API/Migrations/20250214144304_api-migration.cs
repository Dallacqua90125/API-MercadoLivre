using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercicio.API.Migrations
{
    /// <inheritdoc />
    public partial class apimigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Product",
                table: "products",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Product",
                table: "products");
        }
    }
}
