using Microsoft.EntityFrameworkCore.Migrations;

namespace FastMoney.Data.Migrations
{
    public partial class AddedTransactionStatusAndOtp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionOtp",
                table: "Transaction",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TransactionStatus",
                table: "Transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionOtp",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "Transaction");
        }
    }
}
