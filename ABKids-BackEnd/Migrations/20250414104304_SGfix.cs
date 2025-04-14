using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABKids_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SGfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId",
                table: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_SavingsGoals_AccountId",
                table: "SavingsGoals");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "SavingsGoals",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_AccountId",
                table: "SavingsGoals",
                column: "AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId",
                table: "SavingsGoals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId",
                table: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_SavingsGoals_AccountId",
                table: "SavingsGoals");

            migrationBuilder.AlterColumn<int>(
                name: "AccountId",
                table: "SavingsGoals",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_AccountId",
                table: "SavingsGoals",
                column: "AccountId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId",
                table: "SavingsGoals",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
