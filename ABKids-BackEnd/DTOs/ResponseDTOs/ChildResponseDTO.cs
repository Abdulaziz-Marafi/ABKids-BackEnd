namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public class ChildResponseDTO
    {
        public int ChildId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public int? ChildAccountId { get; set; }
        public decimal? Balance { get; set; }
    }
}
