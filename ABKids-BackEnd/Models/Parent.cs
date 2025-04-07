using System.ComponentModel.DataAnnotations.Schema;

namespace ABKids_BackEnd.Models
{
    public class Parent : User
    {

        // Nav properties for related entites
        public ICollection<Child> Children { get; set; } = new List<Child>();
        public ICollection<Task> TasksCreated { get; set; } = new List<Task>();

        // One-to-one relation with Account
        [ForeignKey("Account")]
        public int? AccountId { get; set; } 
        public Account Account { get; set; }
    }
}
