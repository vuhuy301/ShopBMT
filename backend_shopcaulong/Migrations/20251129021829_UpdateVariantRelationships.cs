using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend_shopcaulong.Migrations
{
    public partial class UpdateVariantRelationships : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductVariants_VariantId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_ProductVariants_VariantId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_StockHistories_ProductVariants_ProductVariantId",
                table: "StockHistories");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_VariantId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "ProductVariantId",
                table: "StockHistories",
                newName: "ColorVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_StockHistories_ProductVariantId",
                table: "StockHistories",
                newName: "IX_StockHistories_ColorVariantId");

            migrationBuilder.AddColumn<int>(
                name: "SizeVariantId",
                table: "StockHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductColorVariantId",
                table: "ProductImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ColorVariantId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeVariantId",
                table: "OrderDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ColorVariantId",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SizeVariantId",
                table: "CartItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ProductColorVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductColorVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductColorVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductSizeVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColorVariantId = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductSizeVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductSizeVariants_ProductColorVariants_ColorVariantId",
                        column: x => x.ColorVariantId,
                        principalTable: "ProductColorVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductSizeVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_StockHistories_SizeVariantId",
                table: "StockHistories",
                column: "SizeVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductColorVariantId",
                table: "ProductImages",
                column: "ProductColorVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_ColorVariantId",
                table: "OrderDetails",
                column: "ColorVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_SizeVariantId",
                table: "OrderDetails",
                column: "SizeVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_ColorVariantId",
                table: "CartItems",
                column: "ColorVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_SizeVariantId",
                table: "CartItems",
                column: "SizeVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductColorVariants_ProductId",
                table: "ProductColorVariants",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizeVariants_ColorVariantId",
                table: "ProductSizeVariants",
                column: "ColorVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductSizeVariants_ProductId",
                table: "ProductSizeVariants",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductColorVariants_ColorVariantId",
                table: "CartItems",
                column: "ColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductSizeVariants_SizeVariantId",
                table: "CartItems",
                column: "SizeVariantId",
                principalTable: "ProductSizeVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_ProductColorVariants_ColorVariantId",
                table: "OrderDetails",
                column: "ColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_ProductSizeVariants_SizeVariantId",
                table: "OrderDetails",
                column: "SizeVariantId",
                principalTable: "ProductSizeVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ProductColorVariantId",
                table: "ProductImages",
                column: "ProductColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockHistories_ProductColorVariants_ColorVariantId",
                table: "StockHistories",
                column: "ColorVariantId",
                principalTable: "ProductColorVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockHistories_ProductSizeVariants_SizeVariantId",
                table: "StockHistories",
                column: "SizeVariantId",
                principalTable: "ProductSizeVariants",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductColorVariants_ColorVariantId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_ProductSizeVariants_SizeVariantId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_ProductColorVariants_ColorVariantId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderDetails_ProductSizeVariants_SizeVariantId",
                table: "OrderDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductColorVariants_ProductColorVariantId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_StockHistories_ProductColorVariants_ColorVariantId",
                table: "StockHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_StockHistories_ProductSizeVariants_SizeVariantId",
                table: "StockHistories");

            migrationBuilder.DropTable(
                name: "ProductSizeVariants");

            migrationBuilder.DropTable(
                name: "ProductColorVariants");

            migrationBuilder.DropIndex(
                name: "IX_StockHistories_SizeVariantId",
                table: "StockHistories");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_ProductColorVariantId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_ColorVariantId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_OrderDetails_SizeVariantId",
                table: "OrderDetails");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_ColorVariantId",
                table: "CartItems");

            migrationBuilder.DropIndex(
                name: "IX_CartItems_SizeVariantId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SizeVariantId",
                table: "StockHistories");

            migrationBuilder.DropColumn(
                name: "ProductColorVariantId",
                table: "ProductImages");

            migrationBuilder.DropColumn(
                name: "ColorVariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "SizeVariantId",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "ColorVariantId",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SizeVariantId",
                table: "CartItems");

            migrationBuilder.RenameColumn(
                name: "ColorVariantId",
                table: "StockHistories",
                newName: "ProductVariantId");

            migrationBuilder.RenameIndex(
                name: "IX_StockHistories_ColorVariantId",
                table: "StockHistories",
                newName: "IX_StockHistories_ProductVariantId");

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "CartItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DiscountPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MainImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stock = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetails_VariantId",
                table: "OrderDetails",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_ProductVariants_VariantId",
                table: "CartItems",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_Products_ProductId",
                table: "OrderDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderDetails_ProductVariants_VariantId",
                table: "OrderDetails",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockHistories_ProductVariants_ProductVariantId",
                table: "StockHistories",
                column: "ProductVariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
