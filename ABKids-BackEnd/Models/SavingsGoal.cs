using System.ComponentModel.DataAnnotations.Schema;

namespace ABKids_BackEnd.Models
{
    public class SavingsGoal
    {
        public int SavingsGoalId { get; set; }
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public SavingsGoalStatus Status { get; set; }
        public string? SavingsGoalPicture { get; set; }
        public DateTime? DateCompleted { get; set; }



        // FK to Child
        [ForeignKey("Child")]
        public int ChildId { get; set; }
        public Child Child { get; set; }

        // FK to Account
        [ForeignKey("Account")]
        public int? SavingsGoalAccountId { get; set; }
        public Account? Account { get; set; }
        public enum SavingsGoalStatus
        {
            InProgress,
            Completed,
            Broken
        }
    }
}
