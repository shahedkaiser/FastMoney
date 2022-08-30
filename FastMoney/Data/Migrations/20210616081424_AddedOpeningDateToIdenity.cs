using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FastMoney.Data.Migrations
{
    public partial class AddedOpeningDateToIdenity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OpeningDate",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpeningDate",
                table: "AspNetUsers");
        }
    }
}
