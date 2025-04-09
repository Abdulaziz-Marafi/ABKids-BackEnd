namespace ABKids_BackEnd.DTOs.ResponseDTOs
{
    public abstract class ProfileResponseDTO
    {
        public int UserId { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
    }
}
