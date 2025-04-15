using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.RequestDTOs;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ABKids_BackEnd.Models.Account;
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

        // GET: api/children/tasks (Get All Tasks Assigned to Child User)
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

        // GET: api/children/savings-goals (Get All Savings Goals created by the child)
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

        #endregion

        #region Post Actions

        // POST: api/child/savings-goals (Create a Savings Goal)
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
