using Microsoft.EntityFrameworkCore.Migrations;

namespace CaptchaDestroy.Infrastructure.Migrations
{
    public partial class InitMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TgId = table.Column<long>(type: "INTEGER", nullable: true),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Captchas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsSolved = table.Column<bool>(type: "INTEGER", nullable: false),
                    CaptchaUri = table.Column<string>(type: "TEXT", nullable: true),
                    CaptchaType = table.Column<int>(type: "INTEGER", nullable: false),
                    SolutionString = table.Column<string>(type: "TEXT", nullable: true),
                    AccountId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Captchas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Captchas_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SecretKey",
                table: "Accounts",
                column: "SecretKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Captchas_AccountId",
                table: "Captchas",
                column: "AccountId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Captchas");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
