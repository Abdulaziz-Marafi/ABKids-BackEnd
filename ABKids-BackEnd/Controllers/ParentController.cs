using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.AuthDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static ABKids_BackEnd.Models.Account;
using static ABKids_BackEnd.Models.User;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParentController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ParentController(
            UserManager<User> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion

        #region Actions

        // POST: api/parent/create-child (Create Child Account)
        [HttpPost("create-child")]
        public async Task<IActionResult> CreateChild([FromForm] CreateChildDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify the authenticated user is a Parent
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var authenticatedUser = await _userManager.FindByIdAsync(userId);
            if (authenticatedUser == null || authenticatedUser.Type != UserType.Parent)
            {
                return Unauthorized(new { Message = "Only parents can create child accounts" });
            }

            // Check if email is in use
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email is already in use");
                return BadRequest(ModelState);
            }

            // Upload profile picture if provided
            string profilePicturePath = UploadFile(dto.ProfilePicture);

            // Create the child
            var child = new Child
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Type = UserType.Child,
                ProfilePicture = profilePicturePath,
                ParentId = authenticatedUser.Id, 
                LoyaltyPoints = 0
            };

            var result = await _userManager.CreateAsync(child, dto.Password);

            if (result.Succeeded)
            {
                // Create a default account for the child
                var account = new Account
                {
                    OwnerId = child.Id,
                    OwnerType = AccountOwnerType.Child,
                    Balance = 0m
                };
                child.Account = account;
                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Child account created successfully",
                    ChildId = child.Id,
                    //Email = child.Email,
                    //FirstName = child.FirstName,
                    //LastName = child.LastName,
                    //ParentId = child.ParentId
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return BadRequest(ModelState);
        }

        #endregion

        #region Helper Functions
        [NonAction]
        private string UploadFile(IFormFile image)
        {
            if (image == null)
            {
                return null; // Return null if no file is uploaded (optional profile picture)
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", "ProfilePictures");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                image.CopyTo(fileStream);
            }

            return filePath;
        }
        #endregion

        #region Custom Action Results
        // Helper method for Forbidden response
        [NonAction]
        private IActionResult Forbidden(object value)
        {
            return StatusCode(StatusCodes.Status403Forbidden, value);
        }
        #endregion
    }
}
