using System.ComponentModel.DataAnnotations;

namespace Test.Common.Models
{
    public class CalculateNextNumberRequestModel
    {
        [Required]
        [Range(1, long.MaxValue)]
        public long CurrentNumber { get; set; }
    }
}