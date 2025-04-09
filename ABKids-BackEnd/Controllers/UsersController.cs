using System.Security.Claims;
using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.User;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(
            UserManager<User> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        #endregion


        #region Get Actions 

        // GET: api/users/balance
        [HttpGet("balance")]
        public async Task<IActionResult> GetUserBalance()
        {
            // Verify the authenticated user is a Parent
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { Message = "User not found" });
            }
            // Find the user's account using OwnerId
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(userId));

            var response = new BalanceResponseDTO
            {
                UserId = int.Parse(userId),
                UserType = user.Type.ToString(),
                Balance = account.Balance
            };

            return Ok(response);
        }

        // GET: api/users/Profile
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Invalid authentication" });
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            if (user.Type == UserType.Parent)
            {
                var parent = (Parent)user;
                var response = new ParentProfileResponseDTO
                {
                    UserId = parent.Id,
                    UserType = parent.Type.ToString(),
                    Email = parent.Email,
                    FirstName = parent.FirstName,
                    LastName = parent.LastName,
                    ProfilePicture = parent.ProfilePicture
                    // Futureproofing if we add more fields to Parent
                };
                return Ok(response);
            }
            else if (user.Type == UserType.Child)
            {
                var child = (Child)user;
                var response = new ChildProfileResponseDTO
                {
                    UserId = child.Id,
                    UserType = child.Type.ToString(),
                    Email = child.Email,
                    FirstName = child.FirstName,
                    LastName = child.LastName,
                    ProfilePicture = child.ProfilePicture,
                    ParentId = (int)child.ParentId,
                    LoyaltyPoints = child.LoyaltyPoints
                };
                return Ok(response);
            }

            return BadRequest(new { Message = "Invalid UserType" }); // Fallback, shouldn’t happen
        }
        #endregion
    }
}
