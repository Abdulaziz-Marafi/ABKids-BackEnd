using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.AuthDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using static ABKids_BackEnd.Models.Account;
using static ABKids_BackEnd.Models.User;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        #region Services
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IConfiguration configuration)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }
        #endregion


        #region Parent/Default User

        // POST: api/auth/register (Parent Registration)
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email is in use
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use");
                return BadRequest(ModelState);
            }

            // Check if profile pic was uploaded
            string profilePicturePath = UploadFile(dto.ProfilePicture);

            var parent = new Parent
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Type = UserType.Parent,
                ProfilePicture = profilePicturePath,
                ParentAccountId = null, // Explicitly null,
                
            };

            var result = await _userManager.CreateAsync(parent, dto.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }

            if (result.Succeeded)
            {
                // Create a default account for the parent, linking it to the parent
                var account = new Account
                {
                    OwnerId = parent.Id, // Set the OwnerId to the Parent's Id
                    OwnerType = AccountOwnerType.Parent, // Composite Key with OwnerId
                    Balance = 10000m
                };

                // Link account to parent
                parent.Account = account;
                parent.ParentAccountId = null; // Explicitly set to null before saving account

                // Save account
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                // Update parent with AccountId
                parent.ParentAccountId = account.AccountId;
                await _context.SaveChangesAsync();

                return Ok(new AuthResponseDTO
                {
                    Token = GenerateJwtToken(parent),
                    Role = parent.Type.ToString(),
                    UserId = parent.Id
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        // POST: api/auth/login (Default Login)
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Email");
                return BadRequest(ModelState);
            }
            var result = await _userManager.CheckPasswordAsync(user, dto.Password);
            if (!result)
            {
                ModelState.AddModelError("Password", "Invalid Password");
                return BadRequest(ModelState);
            }
            return Ok(new AuthResponseDTO
            {
                Token = GenerateJwtToken(user),
                Role = user.Type.ToString(),
                UserId = user.Id
            });
        }


        #endregion
        #region Helper Methods
        // Method to upload the profile picture
        [NonAction]
        private string UploadFile(IFormFile image)
        {
            if (image == null)
            {
                return null; // Return null if no file is uploaded (optional profile picture)
            }

            // Use ContentRootPath to get the project root directory
            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", "ProfilePictures");

            // Ensure the directory exists
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            // Return the full file path (TODO: consider returning a relative path instead)
            return filePath;
        }
        // Method to generate string for a givin user
        [NonAction]
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token expires in 1 hour
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
