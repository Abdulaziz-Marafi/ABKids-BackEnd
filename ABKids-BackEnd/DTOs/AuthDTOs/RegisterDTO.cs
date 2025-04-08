namespace ABKids_BackEnd.DTOs.AuthDTOs
{
    public class RegisterDTO
    {
        // Parent register DTO
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public IFormFile? ProfilePicture { get; set; }
    }
}
