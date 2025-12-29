using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_shopcaulong.Migrations
{
    public partial class AddIsActiveAndProfileUpdatedAtToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProfileUpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProfileUpdatedAt",
                table: "Users");
        }
    }
}
