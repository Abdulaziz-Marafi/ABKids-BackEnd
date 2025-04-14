using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABKids_BackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SGForiegnKeyFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_AspNetUsers_OwnerId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts");

            migrationBuilder.AddColumn<int>(
                name: "AccountId1",
                table: "SavingsGoals",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavingsGoals_AccountId1",
                table: "SavingsGoals",
                column: "AccountId1",
                unique: true,
                filter: "[AccountId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_AccountId",
                table: "AspNetUsers",
                column: "AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Child_AccountId",
                table: "AspNetUsers",
                column: "Child_AccountId",
                unique: true,
                filter: "[AccountId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId_OwnerType",
                table: "Accounts",
                columns: new[] { "OwnerId", "OwnerType" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Accounts_AccountId",
                table: "AspNetUsers",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Accounts_Child_AccountId",
                table: "AspNetUsers",
                column: "Child_AccountId",
                principalTable: "Accounts",
                principalColumn: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId1",
                table: "SavingsGoals",
                column: "AccountId1",
                principalTable: "Accounts",
                principalColumn: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Accounts_AccountId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Accounts_Child_AccountId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_SavingsGoals_Accounts_AccountId1",
                table: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_SavingsGoals_AccountId1",
                table: "SavingsGoals");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_AccountId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Child_AccountId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_OwnerId_OwnerType",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "AccountId1",
                table: "SavingsGoals");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts",
                column: "OwnerId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_AspNetUsers_OwnerId",
                table: "Accounts",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
