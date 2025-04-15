using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.RequestDTOs;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.Account;
using static ABKids_BackEnd.Models.LoyaltyTransaction;
using static ABKids_BackEnd.Models.User;
using Task = ABKids_BackEnd.Models.Task;

namespace ABKids_BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChildrenController : ControllerBase
    {
        #region Services

        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ChildrenController(
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

        // GET: api/Children/tasks (Get All Tasks Assigned to Child User)
        [HttpGet("tasks")]
        public async Task<IActionResult> GetTasks()
        {
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can view their tasks" });
            }

            var tasks = await _context.Tasks
                .Where(t => t.ChildId == int.Parse(childId))
                .Select(t => new TaskResponseDTO
                {
                    TaskId = t.TaskId,
                    TaskName = t.TaskName,
                    TaskDescription = t.TaskDescription,
                    TaskPicture = t.TaskPicture,
                    Status = t.Status.ToString(),
                    RewardAmount = t.RewardAmount,
                    DateCreated = t.DateCreated,
                    DateCompleted = t.DateCompleted,
                    ParentId = t.ParentId,
                    ChildId = t.ChildId
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/Children/savings-goals (Get All Savings Goals created by the Child User)
        [HttpGet("savings-goals")]
        public async Task<IActionResult> GetSavingsGoals()
        {
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can view their savings goals" });
            }

            var savingsGoals = await _context.SavingsGoals
                .Include(sg => sg.Account) // Include Account for CurrentBalance
                .Where(sg => sg.ChildId == int.Parse(childId))
                .Select(sg => new SavingsGoalResponseDTO
                {
                    SavingsGoalId = sg.SavingsGoalId,
                    GoalName = sg.GoalName,
                    TargetAmount = sg.TargetAmount,
                    Status = sg.Status.ToString(),
                    SavingsGoalPicture = sg.SavingsGoalPicture,
                    DateCreated = sg.DateCreated,
                    DateCompleted = sg.DateCompleted,
                    ChildId = sg.ChildId,
                    AccountId = (int)sg.AccountId,
                    CurrentBalance = sg.Account != null ? sg.Account.Balance : 0m // Handle null Account
                })
                .ToListAsync();

            return Ok(savingsGoals);
        }

        // Maybe add a Get method just for SG balance alone?

        // GET: api/Children/loyalty-transactions (Get All Loyalty Transactions related to the Child User)
        [HttpGet("loyalty-transactions")]
        public async Task<IActionResult> GetLoyaltyTransactions()
        {
            // Get the authenticated child
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can view loyalty transactions" });
            }

            // Fetch loyalty transactions
            var transactions = await _context.LoyaltyTransactions
                .Where(lt => lt.ChildId == int.Parse(childId))
                .OrderByDescending(lt => lt.DateCreated) // Newest first
                .Select(lt => new LoyaltyTransactionResponseDTO
                {
                    LoyaltyTransactionId = lt.LoyaltyTransactionId,
                    Amount = lt.Amount,
                    LoyaltyTransactionType = lt.TransactionType.ToString(),
                    Description = lt.description,
                    DateCreated = lt.DateCreated,
                    ChildId = lt.ChildId
                })
                .ToListAsync();

            return Ok(transactions);
        }

        #endregion

        #region Post Actions

        // POST: api/Children/savings-goals (Create a Savings Goal)
        [HttpPost("savings-goals")]
        public async Task<IActionResult> CreateSavingsGoal([FromForm] CreateSavingsGoalRequestDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can create savings goals" });
            }

            // Validate TargetAmount range (20-500 KWD)
            if (dto.TargetAmount < 20 || dto.TargetAmount > 500)
            {
                ModelState.AddModelError("TargetAmount", "Target amount must be between 20 and 500 KWD");
                return BadRequest(ModelState);
            }

            // Upload picture if provided
            string picturePath = UploadFile(dto.SavingsGoalPicture, "SavingsGoalPictures");

            // Create savings goal first
            var savingsGoal = new SavingsGoal
            {
                GoalName = dto.GoalName,
                TargetAmount = dto.TargetAmount,
                Status = SavingsGoal.SavingsGoalStatus.InProgress,
                SavingsGoalPicture = picturePath,
                DateCreated = DateTime.UtcNow,
                ChildId = int.Parse(childId)
            };

            _context.SavingsGoals.Add(savingsGoal);
            await _context.SaveChangesAsync(); // Save to get SavingsGoalId

            // Create account with OwnerId = SavingsGoalId
            var goalAccount = new Account
            {
                OwnerId = savingsGoal.SavingsGoalId,
                OwnerType = AccountOwnerType.SavingsGoal,
                Balance = 0m
            };

            _context.Accounts.Add(goalAccount);
            await _context.SaveChangesAsync();

            // Link account to savings goal
            savingsGoal.AccountId = goalAccount.AccountId;
            savingsGoal.Account = goalAccount;
            await _context.SaveChangesAsync();

            var response = new SavingsGoalResponseDTO
            {
                SavingsGoalId = savingsGoal.SavingsGoalId,
                GoalName = savingsGoal.GoalName,
                TargetAmount = savingsGoal.TargetAmount,
                Status = savingsGoal.Status.ToString(),
                SavingsGoalPicture = savingsGoal.SavingsGoalPicture,
                DateCreated = savingsGoal.DateCreated,
                DateCompleted = savingsGoal.DateCompleted,
                ChildId = savingsGoal.ChildId,
                AccountId = (int)savingsGoal.AccountId,
                CurrentBalance = goalAccount.Balance
            };

            return Ok(response);
        }

        // POST: api/Children/{goalId}/deposit (Deposit to a Savings Goal)
        [HttpPost("savings-goals/{goalId}/deposit")]
        public async Task<IActionResult> DepositToSavingsGoal(int goalId, [FromBody] DepositToSavingsGoalRequestDTO dto)
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

            // Fetch authenticated child
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can deposit to savings goals" });
            }

            // Fetch child account
            var childAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(childId) && a.OwnerType == AccountOwnerType.Child);
            if (childAccount == null)
            {
                return BadRequest(new { Message = "Child has no associated account" });
            }

            // Fetch savings goal and its account
            var savingsGoal = await _context.SavingsGoals
                .Include(sg => sg.Account)
                .FirstOrDefaultAsync(sg => sg.SavingsGoalId == goalId && sg.ChildId == int.Parse(childId));
            if (savingsGoal == null)
            {
                return NotFound(new { Message = "Savings goal not found or not associated with this child" });
            }

            if (savingsGoal.Status != SavingsGoal.SavingsGoalStatus.InProgress)
            {
                return BadRequest(new { Message = "Savings goal is not in progress" });
            }

            if (!savingsGoal.AccountId.HasValue || savingsGoal.Account == null)
            {
                return BadRequest(new { Message = "Savings goal has no associated account" });
            }

            // Verify savings goal account's OwnerId and OwnerType
            if (savingsGoal.Account.OwnerId != savingsGoal.SavingsGoalId || savingsGoal.Account.OwnerType != AccountOwnerType.SavingsGoal)
            {
                return BadRequest(new { Message = "Savings goal account has incorrect OwnerId or OwnerType" });
            }

            // Calculate remaining amount needed
            var remainingAmount = savingsGoal.TargetAmount - savingsGoal.Account.Balance;
            if (remainingAmount <= 0)
            {
                return BadRequest(new { Message = "Savings goal is already at or above target amount" });
            }

            // Cap deposit at remaining amount
            var depositAmount = Math.Min(dto.Amount, remainingAmount);

            // Verify sufficient funds
            if (childAccount.Balance < depositAmount)
            {
                return BadRequest(new { Message = "Insufficient funds in child account" });
            }

            // Perform deposit
            childAccount.Balance -= depositAmount;
            savingsGoal.Account.Balance += depositAmount;

            // Create transaction: Child → SavingsGoal
            var depositTransaction = new Transaction
            {
                Amount = depositAmount,
                DateCreated = DateTime.UtcNow,
                SenderAccountId = childAccount.AccountId,
                SenderAccount = childAccount,
                SenderType = (Transaction.AccountOwnerType)AccountOwnerType.Child,
                ReceiverAccountId = savingsGoal.AccountId.Value,
                ReceiverAccount = savingsGoal.Account,
                ReceiverType = (Transaction.AccountOwnerType)AccountOwnerType.SavingsGoal
            };

            _context.Transactions.Add(depositTransaction);

            // Check for completion
            if (savingsGoal.Account.Balance >= savingsGoal.TargetAmount)
            {
                savingsGoal.Status = SavingsGoal.SavingsGoalStatus.Completed;
                savingsGoal.DateCompleted = DateTime.UtcNow;

                // Transfer balance back to child
                var returnAmount = savingsGoal.Account.Balance;
                childAccount.Balance += returnAmount;
                savingsGoal.Account.Balance = 0m;

                // Create transaction: SavingsGoal → Child
                var completionTransaction = new Transaction
                {
                    Amount = returnAmount,
                    DateCreated = DateTime.UtcNow,
                    SenderAccountId = savingsGoal.AccountId.Value,
                    SenderAccount = savingsGoal.Account,
                    SenderType = (Transaction.AccountOwnerType)AccountOwnerType.SavingsGoal,
                    ReceiverAccountId = childAccount.AccountId,
                    ReceiverAccount = childAccount,
                    ReceiverType = (Transaction.AccountOwnerType)AccountOwnerType.Child
                };

                _context.Transactions.Add(completionTransaction);

                // Award loyalty points: TargetAmount / 2 * 10
                var loyaltyPoints = (int)(savingsGoal.TargetAmount / 2);
                savingsGoal.Child.LoyaltyPoints += loyaltyPoints;

                // Create loyalty transaction
                var loyaltyTransaction = new LoyaltyTransaction
                {
                    Amount = loyaltyPoints,
                    TransactionType = LoyaltyTransactionType.Earned,
                    description = $"Earned {loyaltyPoints} points for completing savings goal '{savingsGoal.GoalName}' on {savingsGoal.DateCompleted:yyyy-MM-dd}",
                    DateCreated = DateTime.UtcNow,
                    ChildId = savingsGoal.ChildId,
                    Child = savingsGoal.Child
                };

                _context.LoyaltyTransactions.Add(loyaltyTransaction);
            }

            await _context.SaveChangesAsync();

            var response = new SavingsGoalResponseDTO
            {
                SavingsGoalId = savingsGoal.SavingsGoalId,
                GoalName = savingsGoal.GoalName,
                TargetAmount = savingsGoal.TargetAmount,
                Status = savingsGoal.Status.ToString(),
                SavingsGoalPicture = savingsGoal.SavingsGoalPicture,
                DateCreated = savingsGoal.DateCreated,
                DateCompleted = savingsGoal.DateCompleted,
                ChildId = savingsGoal.ChildId,
                AccountId = (int)savingsGoal.AccountId,
                CurrentBalance = savingsGoal.Account.Balance,
                Message = depositAmount < dto.Amount ? $"Deposit capped at {depositAmount:F2} KWD to meet target" : null
            };

            return Ok(response);
        }

        // POST: api/Children/savings-goals/{goalId}/break (Break a Savings Goal)
        [HttpPost("savings-goals/{goalId}/break")]
        public async Task<IActionResult> BreakSavingsGoal(int goalId)
        {
            // Get the authenticated child
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can break savings goals" });
            }

            // Fetch child account using composite key
            var childAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.OwnerId == int.Parse(childId) && a.OwnerType == AccountOwnerType.Child);
            if (childAccount == null)
            {
                return BadRequest(new { Message = "Child has no associated account" });
            }

            // Fetch savings goal and its account
            var savingsGoal = await _context.SavingsGoals
                .Include(sg => sg.Account)
                .FirstOrDefaultAsync(sg => sg.SavingsGoalId == goalId && sg.ChildId == int.Parse(childId));
            if (savingsGoal == null)
            {
                return NotFound(new { Message = "Savings goal not found or not associated with this child" });
            }

            if (savingsGoal.Status != SavingsGoal.SavingsGoalStatus.InProgress)
            {
                return BadRequest(new { Message = "Savings goal is not in progress" });
            }

            if (!savingsGoal.AccountId.HasValue || savingsGoal.Account == null)
            {
                return BadRequest(new { Message = "Savings goal has no associated account" });
            }

            // Verify savings goal account's composite key
            if (savingsGoal.Account.OwnerId != savingsGoal.SavingsGoalId || savingsGoal.Account.OwnerType != AccountOwnerType.SavingsGoal)
            {
                return BadRequest(new { Message = "Savings goal account has incorrect OwnerId or OwnerType" });
            }

            // Transfer balance to child account
            var transferAmount = savingsGoal.Account.Balance;
            if (transferAmount > 0)
            {
                childAccount.Balance += transferAmount;
                savingsGoal.Account.Balance = 0m;

                // Create transaction: SavingsGoal → Child
                var transaction = new Transaction
                {
                    Amount = transferAmount,
                    DateCreated = DateTime.UtcNow,
                    SenderAccountId = savingsGoal.AccountId.Value,
                    SenderAccount = savingsGoal.Account,
                    SenderType = (Transaction.AccountOwnerType)AccountOwnerType.SavingsGoal,
                    ReceiverAccountId = childAccount.AccountId,
                    ReceiverAccount = childAccount,
                    ReceiverType = (Transaction.AccountOwnerType)AccountOwnerType.Child
                };

                _context.Transactions.Add(transaction);
            }

            // Update savings goal status
            savingsGoal.Status = SavingsGoal.SavingsGoalStatus.Broken;
            savingsGoal.DateCompleted = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new SavingsGoalResponseDTO
            {
                SavingsGoalId = savingsGoal.SavingsGoalId,
                GoalName = savingsGoal.GoalName,
                TargetAmount = savingsGoal.TargetAmount,
                Status = savingsGoal.Status.ToString(),
                SavingsGoalPicture = savingsGoal.SavingsGoalPicture,
                DateCreated = savingsGoal.DateCreated,
                DateCompleted = savingsGoal.DateCompleted,
                ChildId = savingsGoal.ChildId,
                AccountId = (int)savingsGoal.AccountId,
                CurrentBalance = savingsGoal.Account.Balance,
                Message = transferAmount > 0 ? $"Transferred {transferAmount:F2} KWD back to child account" : "Savings goal broken with no funds to transfer"
            };

            return Ok(response);
        }
        #endregion

        #region Put Actions

        // PUT: api/child/tasks/{taskId}/complete (Mark a Task as Complete)
        [HttpPut("tasks/{taskId}/complete")]
        public async Task<IActionResult> CompleteTask(int taskId)
        {
            var childId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var child = await _userManager.FindByIdAsync(childId);
            if (child == null || child.Type != UserType.Child)
            {
                return Unauthorized(new { Message = "Only children can complete tasks" });
            }

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.ChildId == int.Parse(childId));

            if (task == null)
            {
                return NotFound(new { Message = "Task not found or not assigned to this child" });
            }

            if (task.Status != Task.TaskStatus.Ongoing)
            {
                return BadRequest(new { Message = "Task cannot be completed as it is not in Ongoing status" });
            }

            // Update task status and completion date
            task.Status = Task.TaskStatus.Verify;
            task.DateCompleted = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Prepare response
            var response = new TaskResponseDTO
            {
                TaskId = task.TaskId,
                TaskName = task.TaskName,
                TaskDescription = task.TaskDescription,
                TaskPicture = task.TaskPicture,
                Status = task.Status.ToString(),
                RewardAmount = task.RewardAmount,
                DateCreated = task.DateCreated,
                DateCompleted = task.DateCompleted,
                ParentId = task.ParentId,
                ChildId = task.ChildId
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
