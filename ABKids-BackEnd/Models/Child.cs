using System.ComponentModel.DataAnnotations.Schema;

namespace ABKids_BackEnd.Models
{
    public class Child : User
    {
        public int LoyaltyPoints { get; set; }

        // Nav properties
        public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
        public ICollection<Task> TasksAssigned { get; set; } = new List<Task>();
        public ICollection<LoyaltyTransaction> LoyaltyTransactions { get; set;} = new List<LoyaltyTransaction>();


        // One-to-one relation with Account
        [ForeignKey("Account")]
        public int? ChildAccountId { get; set; }
        public Account? Account { get; set; }

        // One(Parent)-to-many(Child) relation
        [ForeignKey("Parent")]
        public int? ParentId { get; set; }
        public Parent? Parent { get; set; }
    }
}
