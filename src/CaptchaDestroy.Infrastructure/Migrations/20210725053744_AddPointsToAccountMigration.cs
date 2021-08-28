using Microsoft.EntityFrameworkCore.Migrations;

namespace CaptchaDestroy.Infrastructure.Migrations
{
    public partial class AddPointsToAccountMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Points",
                table: "Accounts",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Points",
                table: "Accounts");
        }
    }
}
