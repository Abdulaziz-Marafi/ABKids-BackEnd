namespace ABKids_BackEnd.DTOs.AuthDTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; }
        public string Role { get; set; }

        public int UserId { get; set; }
    }
}
