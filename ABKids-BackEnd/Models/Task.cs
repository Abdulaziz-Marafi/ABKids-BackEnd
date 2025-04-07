using System.ComponentModel.DataAnnotations.Schema;

namespace ABKids_BackEnd.Models
{
    public class Task
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public string? TaskDescription { get; set; }
        public string? TaskPicture { get; set; }
        public TaskStatus Status { get; set; }
        public decimal RewardAmount { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateCompleted { get; set; }


        // FK with Parent
        [ForeignKey("Parent")]
        public int ParentId { get; set; }
        public Parent Parent { get; set; }

        // FK with Child
        [ForeignKey("Child")]
        public int ChildId { get; set; }
        public Child Child { get; set; }

        public enum TaskStatus
        {
            Ongoing,
            Verify,
            Completed,
            Rejected
        }
    }
}
