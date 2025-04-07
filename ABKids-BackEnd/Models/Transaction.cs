using static ABKids_BackEnd.Models.Account;

namespace ABKids_BackEnd.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateCreated { get; set; }
        //public string Description { get; set; } = string.Empty;
       
        
        



        // Sender (FK) (Parent/Child/SavingsGoal)
        public int SenderAccountId { get; set; }
        public Account SenderAccount { get; set; }
        public AccountOwnerType SenderType { get; set; }

        // Receiver (FK) (Parent/Child/SavingsGoal)
        public int ReceiverAccountId { get; set; }
        public Account ReceiverAccount { get; set; }
        public AccountOwnerType ReceiverType { get; set; }


        public enum AccountOwnerType
        {
            Parent,
            Child,
            SavingsGoal
        }

    }
}
