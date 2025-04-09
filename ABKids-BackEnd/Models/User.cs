using Microsoft.AspNetCore.Identity;

namespace ABKids_BackEnd.Models
{
    public abstract class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public UserType Type { get; set; } // Discriminator Property



        public enum UserType
        {
            Parent,
            Child
        }

    }
 
}
