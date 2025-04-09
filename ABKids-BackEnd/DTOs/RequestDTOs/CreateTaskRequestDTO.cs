namespace ABKids_BackEnd.DTOs.RequestDTOs
{
    public class CreateTaskRequestDTO
    {
        public string TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public IFormFile? TaskPicture { get; set; } 
        public decimal RewardAmount { get; set; }
        public int ChildId { get; set; }
    }
}
