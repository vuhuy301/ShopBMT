namespace backend_shopcaulong.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }   // Admin, Staff, Customer
        public ICollection<User> Users { get; set; }
    }
}
