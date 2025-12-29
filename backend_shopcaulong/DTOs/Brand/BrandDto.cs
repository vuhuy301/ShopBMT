namespace backend_shopcaulong.DTOs.Brand
{
    public class BrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // Số sản phẩm thuộc brand
        public int ProductCount { get; set; }

        public bool IsActive { get; set; }
    }

 
}
