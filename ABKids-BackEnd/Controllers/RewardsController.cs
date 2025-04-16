using System.Security.Claims;
using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.RequestDTOs;
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
    public class RewardsController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public RewardsController(
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
        [HttpGet]
        public async Task<IActionResult> GetAllRewards()
        {
            //// Get authenticated child
            //var childId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //if (string.IsNullOrEmpty(childId))
            //{
            //    return Unauthorized(new { Message = "User not authenticated" });
            //}

            //var child = await _userManager.FindByIdAsync(childId);
            //if (child == null || child.Type != UserType.Child)
            //{
            //    return Unauthorized(new { Message = "Only children can view rewards" });
            //}

            var rewards = await _context.Rewards
                .Select(r => new RewardResponseDTO
                {
                    RewardId = r.RewardId,
                    RewardName = r.RewardName,
                    RewardDescription = r.RewardDescription,
                    RewardPrice = r.RewardPrice,
                    RewardPicture = r.RewardPicture
                })
                .ToListAsync();

            return Ok(rewards);
        }
        #endregion

        #region Post Actions
        [HttpPost("create")]
        [AllowAnonymous] // Neutral, backend-only
        public async Task<IActionResult> CreateReward([FromForm] CreateRewardRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate price
            if (dto.RewardPrice <= 0)
            {
                return BadRequest(new { Message = "Reward price must be positive" });
            }

            var rewardPicFilePath = UploadFile(dto.RewardPicture, "RewardsPictures");

            var reward = new Reward
            {
                RewardName = dto.RewardName,
                RewardDescription = dto.RewardDescription,
                RewardPrice = dto.RewardPrice,
                RewardPicture = rewardPicFilePath
            };

            _context.Rewards.Add(reward);
            await _context.SaveChangesAsync();

            var response = new RewardResponseDTO
            {
                RewardId = reward.RewardId,
                RewardName = reward.RewardName,
                RewardDescription = reward.RewardDescription,
                RewardPrice = reward.RewardPrice,
                RewardPicture = reward.RewardPicture
            };

            return CreatedAtAction(nameof(GetAllRewards), new { id = reward.RewardId }, response);
        }
        #endregion

        #region Helper Functions
        [NonAction]
        private string UploadFile(IFormFile image, string folder)
        {
            if (image == null)
            {
                return null; // Return null if no file is uploaded
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.ContentRootPath, "Uploads", folder);
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
    }
}
