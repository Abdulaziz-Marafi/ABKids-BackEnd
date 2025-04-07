using System.ComponentModel.DataAnnotations.Schema;

namespace ABKids_BackEnd.Models
{
    public class LoyaltyTransaction
    {
        public int LoyaltyTransactionId { get; set; }
        public int Amount { get; set; }
        public LoyaltyTransactionType TransactionType { get; set; } // Earned or Spent

        public string? description { get; set; }
        public DateTime DateCreated { get; set; }

        // FK to Child
        [ForeignKey("Child")]
        public int ChildId { get; set; }
        public Child Child { get; set; }

        public enum LoyaltyTransactionType
        {
            Earned,
            Spent
        }
    }
}
