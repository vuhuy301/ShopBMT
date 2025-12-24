namespace backend_shopcaulong.DTOs.Payment
{
    public class SepayWebhookDto
    {
        public int Id { get; set; }
        public string Gateway { get; set; }
        public string TransactionDate { get; set; }
        public string AccountNumber { get; set; }
        public string? Code { get; set; }
        public string Content { get; set; }
        public string TransferType { get; set; }
        public long TransferAmount { get; set; }
        public long Accumulated { get; set; }
        public string? SubAccount { get; set; }
        public string ReferenceCode { get; set; }
        public string? Description { get; set; }
    }
}
