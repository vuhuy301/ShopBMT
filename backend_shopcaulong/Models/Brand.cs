namespace backend_shopcaulong.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Product> Products { get; set; }

    }
}
