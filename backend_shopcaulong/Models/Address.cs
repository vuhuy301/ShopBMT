namespace backend_shopcaulong.Models
{
    public class Address
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public string Detail { get; set; }
        public bool IsDefault { get; set; } 
    }
}
