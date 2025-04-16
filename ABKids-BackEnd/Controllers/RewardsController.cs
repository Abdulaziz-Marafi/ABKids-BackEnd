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
using static ABKids_BackEnd.Models.Account;
using static ABKids_BackEnd.Models.LoyaltyTransaction;
using static ABKids_BackEnd.Models.User;
using static ABKids_BackEnd.Models.Transaction;

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

        [HttpPost("convert-points")]
        [Authorize]
        public async Task<IActionResult> ConvertPointsToMoney([FromBody] ConvertPointsRequestDTO dto)
        {
            // Get authenticated child
            var childId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(childId))
            {
                return Unauthorized(new { Message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(childId);
            if (user == null || user.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can convert loyalty points" });
            }

            var child = (Child)user; // Cast for LoyaltyPoints

            // Validate points
            if (dto.PointsToConvert <= 0)
            {
                return BadRequest(new { Message = "Points to convert must be positive" });
            }

            if (dto.PointsToConvert % 10 != 0)
            {
                return BadRequest(new { Message = "Points to convert must be a multiple of 10" });
            }

            if (child.LoyaltyPoints < dto.PointsToConvert)
            {
                return BadRequest(new { Message = $"Insufficient loyalty points. Required: {dto.PointsToConvert}, Available: {child.LoyaltyPoints}" });
            }

            // Fetch child account using composite key
            var childAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(childId) && a.OwnerType == Account.AccountOwnerType.Child);
            if (childAccount == null)
            {
                return BadRequest(new { Message = "Child has no associated account" });
            }

            // Fetch RewardSystem account
            var rewardSystemAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerType == Account.AccountOwnerType.RewardSystem);
            if (rewardSystemAccount == null)
            {
                return StatusCode(500, new { Message = "RewardSystem account not configured" });
            }

            // Calculate money (10 points = 1 KWD)
            var moneyReceived = dto.PointsToConvert / 10m;

            // Deduct points and add money
            child.LoyaltyPoints -= dto.PointsToConvert;
            childAccount.Balance += moneyReceived;

            // Create loyalty transaction
            var loyaltyTransaction = new LoyaltyTransaction
            {
                Amount = dto.PointsToConvert,
                TransactionType = LoyaltyTransactionType.Spent,
                description = $"Converted {dto.PointsToConvert} loyalty points to {moneyReceived:F2} KWD",
                DateCreated = DateTime.UtcNow,
                ChildId = int.Parse(childId),
                Child = child
            };

            // Create transaction
            var transaction = new Transaction
            {
                Amount = moneyReceived,
                DateCreated = DateTime.UtcNow,
                SenderAccountId = rewardSystemAccount.AccountId, // Use seeded account
                SenderAccount = rewardSystemAccount,
                SenderType = Transaction.AccountOwnerType.RewardSystem,
                ReceiverAccountId = childAccount.AccountId,
                ReceiverAccount = childAccount,
                ReceiverType = Transaction.AccountOwnerType.Child
            };

            _context.LoyaltyTransactions.Add(loyaltyTransaction);
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var response = new ConvertPointsResponseDTO
            {
                PointsConverted = dto.PointsToConvert,
                MoneyReceived = moneyReceived,
                RemainingPoints = child.LoyaltyPoints,
                NewBalance = childAccount.Balance,
                Message = $"Successfully converted {dto.PointsToConvert} points to {moneyReceived:F2} KWD"
            };

            return Ok(response);
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
