using Microsoft.EntityFrameworkCore;

namespace backend_shopcaulong.Models
{
    public class ShopDbContext : DbContext
    {
        public ShopDbContext(DbContextOptions<ShopDbContext> options)
            : base(options)
        {
        }

        // ====== DbSet ======
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductDetail> ProductDetails { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<StockHistory> StockHistories { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        public DbSet<Banner> Banners { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ProductColorVariant> ProductColorVariants { get; set; }
        public DbSet<ProductSizeVariant> ProductSizeVariants { get; set; }
        public DbSet<Payment> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== CATEGORY & PRODUCT =====
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== BRAND & PRODUCT =====
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Brand)
                .WithMany(b => b.Products)
                .HasForeignKey(p => p.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== PRODUCT - IMAGES =====
            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== PRODUCT - DETAILS =====
            modelBuilder.Entity<ProductDetail>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.Details)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== PRODUCT - COLOR VARIANT =====
            modelBuilder.Entity<ProductColorVariant>()
                .HasOne(cv => cv.Product)
                .WithMany(p => p.ColorVariants)
                .HasForeignKey(cv => cv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== COLOR VARIANT - SIZE VARIANT =====
            modelBuilder.Entity<ProductSizeVariant>()
                .HasOne(sv => sv.ColorVariant)
                .WithMany(cv => cv.Sizes)
                .HasForeignKey(sv => sv.ColorVariantId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== STOCK HISTORY =====
            modelBuilder.Entity<StockHistory>()
                .HasOne(sh => sh.Product)
                .WithMany(p => p.StockHistories)
                .HasForeignKey(sh => sh.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StockHistory>()
                .HasOne(sh => sh.ColorVariant)
                .WithMany()
                .HasForeignKey(sh => sh.ColorVariantId)
                .OnDelete(DeleteBehavior.Restrict);


            // ===== ORDER & ORDERDETAIL =====
            modelBuilder.Entity<Order>()
                .HasMany(o => o.Items)
                .WithOne(od => od.Order)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.ColorVariant)
                .WithMany()
                .HasForeignKey(od => od.ColorVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.SizeVariant)
                .WithMany()
                .HasForeignKey(od => od.SizeVariantId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== USER - ORDER =====
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ===== PRODUCT PROMOTION =====
            modelBuilder.Entity<ProductPromotion>()
                .HasKey(pp => new { pp.ProductId, pp.PromotionId });

            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.ProductId);

            modelBuilder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Promotion)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.PromotionId);
        }

    }
}
