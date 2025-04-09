namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class TransactionResponseDTO
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        public int SenderAccountId { get; set; }
        public AccountOwnerType SenderType { get; set; }
        public int ReceiverAccountId { get; set; }
        public AccountOwnerType ReceiverType { get; set; }
        public decimal NewReceiverBalance { get; set; }
        
        


        public enum AccountOwnerType
        {
            Parent,
            Child,
            SavingsGoal
        }
    }
}
