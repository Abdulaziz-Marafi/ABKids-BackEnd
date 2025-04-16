namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class RewardResponseDTO
    {
        public int RewardId { get; set; }
        public string RewardName { get; set; }
        public string? RewardDescription { get; set; }
        public int RewardPrice { get; set; } // In lotalty points
        public string? RewardPicture { get; set; }
    }
}
