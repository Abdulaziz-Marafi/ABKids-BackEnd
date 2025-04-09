namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class ChildProfileResponseDTO : ProfileResponseDTO
    {
        public int ParentId { get; set; }
        public int LoyaltyPoints { get; set; }
    }
}
