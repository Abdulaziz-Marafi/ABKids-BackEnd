namespace ABKids_BackEnd.DTOs.RequestDTOs
{
    public class CreateSavingsGoalRequestDTO
    {
        public string GoalName { get; set; }
        public decimal TargetAmount { get; set; }
        public IFormFile? SavingsGoalPicture { get; set; }
    }
}
