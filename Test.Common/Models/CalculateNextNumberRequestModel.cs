using System.ComponentModel.DataAnnotations;

namespace Test.Common.Models
{
    public class CalculateNextNumberRequestModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public long CurrentNumber { get; set; }
    }
}