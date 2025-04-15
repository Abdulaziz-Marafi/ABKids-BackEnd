namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class LoyaltyTransactionResponseDTO
    {
        public int LoyaltyTransactionId { get; set; }
        public int Amount { get; set; }
        public string LoyaltyTransactionType { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreated { get; set; }
        public int ChildId { get; set; }
    }
}
