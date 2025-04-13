using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.User;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public TransactionsController(
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

        // GET: api/transactions
        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || (user.Type != UserType.Parent && user.Type != UserType.Child))
            {
                return Unauthorized(new { Message = "Only parents and children can view transactions" });
            }

            // Find the user's account
            var account = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(userId));
            if (account == null)
            {
                return NotFound(new { Message = "No account found for this user" });
            }

            // Get transactions where the user is sender or receiver
            var transactions = await _context.Transactions
                .Where(t => t.SenderAccountId == account.AccountId || t.ReceiverAccountId == account.AccountId)
                .Select(t => new TransactionResponseDTO
                {
                    TransactionId = t.TransactionId,
                    Amount = t.Amount,
                    DateCreated = t.DateCreated,
                    SenderAccountId = t.SenderAccountId,
                    SenderType = t.SenderType.ToString(),
                    ReceiverAccountId = t.ReceiverAccountId,
                    ReceiverType = t.ReceiverType.ToString()
                })
                .OrderByDescending(t => t.DateCreated) // Newest first
                .ToListAsync();

            return Ok(transactions);
        }
            #endregion
        }
}
