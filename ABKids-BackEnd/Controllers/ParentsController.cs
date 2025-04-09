﻿using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.AuthDTOs;
using ABKids_BackEnd.DTOs.RequestDTOs;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.Account;
using static ABKids_BackEnd.Models.User;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ParentsController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ParentsController(
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

        // POST: api/parent/deposit-to-child (Deposit to Child Account)
        [HttpPost("deposit-to-child")]
        public async Task<IActionResult> DepositToChildAccount(ChildDepositRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validate amount
            if (dto.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Deposit amount must be positive");
                return BadRequest(ModelState);
            }

            // Get the authenticated parent's ID and details
            var parentId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var parent = await _userManager.FindByIdAsync(parentId);
            if (parent == null || parent.Type != UserType.Parent)
            {
                return Unauthorized(new { Message = "Only parents can deposit to child accounts" });
            }

            // Load parent's account
            var parentAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(parentId));
            if (parentAccount == null)
            {
                return BadRequest(new { Message = "Parent has no associated account" });
            }

            // Check if parent has sufficient balance (optional rule)
            if (parentAccount.Balance < dto.Amount)
            {
                return BadRequest(new { Message = "Insufficient funds in parent account" });
            }else if (dto.Amount < 0.01m)
            {
                return BadRequest(new { Message = "Cannot deposit an amount less than 0.01" });
            }

            // Find the child and verify they belong to this parent
            var child = await _context.Users.OfType<Child>()
                .Include(c => c.Account)
                .FirstOrDefaultAsync(c => c.Id == dto.ChildId && c.ParentId == int.Parse(parentId));

            if (child == null)
            {
                return NotFound(new { Message = "Child not found or not associated with this parent" });
            }

            if (child.Account == null)
            {
                return BadRequest(new { Message = "Child has no associated account" });
            }

            // Update balances
            parentAccount.Balance -= dto.Amount;
            child.Account.Balance += dto.Amount;

            // Create a transaction record
            var transaction = new Transaction
            {
                Amount = dto.Amount,
                DateCreated = DateTime.UtcNow,
                SenderAccountId = parentAccount.AccountId,
                SenderAccount = parentAccount,
                SenderType = Transaction.AccountOwnerType.Parent,
                ReceiverAccountId = child.Account.AccountId,
                ReceiverAccount = child.Account,
                ReceiverType = Transaction.AccountOwnerType.Child
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            var response = new TransactionResponseDTO
            {
                TransactionId = transaction.TransactionId,
                Amount = dto.Amount,
                DateCreated = transaction.DateCreated,
                SenderAccountId = parentAccount.AccountId,
                SenderType = TransactionResponseDTO.AccountOwnerType.Parent,
                ReceiverAccountId = child.Account.AccountId,
                ReceiverType = TransactionResponseDTO.AccountOwnerType.Child,
                NewReceiverBalance = child.Account.Balance
            };

            return Ok(response);
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
