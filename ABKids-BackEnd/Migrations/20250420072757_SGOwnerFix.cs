using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABKids_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SGOwnerFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_SavingsGoals_SavingsGoalId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_SavingsGoalId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "SavingsGoalId",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SavingsGoalId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_SavingsGoalId",
                table: "Accounts",
                column: "SavingsGoalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_SavingsGoals_SavingsGoalId",
                table: "Accounts",
                column: "SavingsGoalId",
                principalTable: "SavingsGoals",
                principalColumn: "SavingsGoalId");
        }
    }
}
