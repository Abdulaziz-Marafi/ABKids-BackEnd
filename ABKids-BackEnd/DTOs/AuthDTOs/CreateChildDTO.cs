namespace ABKids_BackEnd.DTOs.AuthDTOs
{
    public class CreateChildDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile? ProfilePicture { get; set; }
       // public int ParentId { get; set; }
    }
}
