namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class RedeemRewardResponseDTO
    {
        public int RewardId { get; set; }
        public string RewardName { get; set; }
        public int PointsSpent { get; set; }
        public int RemainingPoints { get; set; }
        public string? Message { get; set; }
    }
}
