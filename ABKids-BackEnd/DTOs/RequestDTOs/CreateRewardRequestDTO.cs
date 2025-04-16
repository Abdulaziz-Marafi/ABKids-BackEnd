namespace ABKids_BackEnd.DTOs.RequestDTOs
{
    public class CreateRewardRequestDTO
    {
        public string RewardName { get; set; }
        public string? RewardDescription { get; set; }
        public int RewardPrice { get; set; } // In lotalty points
        public string? RewardPicture { get; set; }
    }
}
