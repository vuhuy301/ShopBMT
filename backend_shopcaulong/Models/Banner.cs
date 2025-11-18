namespace backend_shopcaulong.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string? Link { get; set; }
        public bool IsActive { get; set; }
    }
}
