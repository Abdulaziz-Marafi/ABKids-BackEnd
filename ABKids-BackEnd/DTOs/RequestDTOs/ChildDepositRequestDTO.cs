using System.ComponentModel.DataAnnotations;

namespace ABKids_BackEnd.DTOs.RequestDTOs
{
    public class ChildDepositRequestDTO
    {
        // Range of 0.01 to 10000.00
        [Range(0.01, 10000.00, ErrorMessage = "Amount must be between 0.01 and 10000.00")]
        public decimal Amount { get; set; }
        public int? ChildId { get; set; }
    }
}
