using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_shopcaulong.Migrations
{
    public partial class AddStockHistoryRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_StockHistories_ProductId",
                table: "StockHistories",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockHistories_Products_ProductId",
                table: "StockHistories",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockHistories_Products_ProductId",
                table: "StockHistories");

            migrationBuilder.DropIndex(
                name: "IX_StockHistories_ProductId",
                table: "StockHistories");
        }
    }
}
