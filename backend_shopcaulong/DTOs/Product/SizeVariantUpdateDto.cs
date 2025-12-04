namespace backend_shopcaulong.DTOs.Product
{
    public class SizeVariantUpdateDto
    {
        public int Id { get; set; } // 0 = thêm mới
        public string Size { get; set; }
        public int Stock { get; set; }
    }

}
