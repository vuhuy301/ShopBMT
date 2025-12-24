using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_shopcaulong.Migrations
{
    public partial class AddCustomerEmailToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerEmail",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerEmail",
                table: "Orders");
        }
    }
}
