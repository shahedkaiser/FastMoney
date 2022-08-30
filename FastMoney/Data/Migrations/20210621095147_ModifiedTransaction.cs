using Microsoft.EntityFrameworkCore.Migrations;

namespace FastMoney.Data.Migrations
{
    public partial class ModifiedTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_BeneficiaryId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_BeneficiaryId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BeneficiaryId",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryAccountNumber",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryBankName",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryEmail",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryName",
                table: "Transaction",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryPhoneNumber",
                table: "Transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BeneficiaryAccountNumber",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BeneficiaryBankName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BeneficiaryEmail",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BeneficiaryName",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "BeneficiaryPhoneNumber",
                table: "Transaction");

            migrationBuilder.AddColumn<string>(
                name: "BeneficiaryId",
                table: "Transaction",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BeneficiaryId",
                table: "Transaction",
                column: "BeneficiaryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_BeneficiaryId",
                table: "Transaction",
                column: "BeneficiaryId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
