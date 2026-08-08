using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TopHeroesBot.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
        UPDATE Accounts
        SET "Order" = rowid;
    """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "Accounts");
        }
    }
}
