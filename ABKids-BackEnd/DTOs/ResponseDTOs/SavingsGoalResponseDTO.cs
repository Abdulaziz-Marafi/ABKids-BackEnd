namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class SavingsGoalResponseDTO
    {
        public int SavingsGoalId { get; set; }
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public string Status { get; set; }
        public string? SavingsGoalPicture { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }
        public int ChildId { get; set; }
        public int AccountId { get; set; }
        public decimal CurrentBalance { get; set; }

        public string? Message { get; set; }
    }
}
