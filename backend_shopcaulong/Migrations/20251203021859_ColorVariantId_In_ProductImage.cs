using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_shopcaulong.Migrations
{
    public partial class ColorVariantId_In_ProductImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ProductColorVariantId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "ProductSizeVariants");

            migrationBuilder.RenameColumn(
                name: "ProductColorVariantId",
                table: "ProductImages",
                newName: "ColorVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductColorVariantId",
                table: "ProductImages",
                newName: "IX_ProductImages_ColorVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ColorVariantId",
                table: "ProductImages",
                column: "ColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ColorVariantId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ColorVariantId",
                table: "ProductImages",
                newName: "ProductColorVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ColorVariantId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductColorVariantId");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "ProductSizeVariants",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ProductColorVariantId",
                table: "ProductImages",
                column: "ProductColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id");
        }
    }
}
