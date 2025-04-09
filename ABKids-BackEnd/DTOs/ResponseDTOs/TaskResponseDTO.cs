namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class TaskResponseDTO
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public string? TaskPicture { get; set; }
        public string Status { get; set; }
        public decimal RewardAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public int ParentId { get; set; }
        public int ChildId { get; set; }
    }
}
