namespace ABKids_BackEnd.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public AccountOwnerType OwnerType { get; set; } 
        public decimal Balance { get; set; }

        // One to One relation with Parent/Child/SavingsGoal
        public int OwnerId { get; set; } // Foreign key to Parent, Child, or SavingsGoal
        public Parent? ParentOwner { get; set; } // If OwnerType is Parent
        public Child? ChildOwner { get; set; }   // If OwnerType is Child
        public SavingsGoal? SavingsGoal { get; set; } // If OwnerType is SavingsGoal

        // Transaction related
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>(); 
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();





        public enum AccountOwnerType
        {
            Parent,
            Child,
            SavingsGoal
        }

    }
}
