using ABKids_BackEnd.Data;
using ABKids_BackEnd.DTOs.ResponseDTOs;
using ABKids_BackEnd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        #endregion

        #region Post Actions




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
