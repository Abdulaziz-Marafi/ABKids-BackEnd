namespace ABKids_BackEnd.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public AccountOwnerType OwnerType { get; set; } 
        public decimal Balance { get; set; }

        // Continue Relations 




        public enum AccountOwnerType
        {
            Parent,
            Child,
            SavingsGoal
        }

    }
}
