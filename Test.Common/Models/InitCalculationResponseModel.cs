using System;
using System.ComponentModel.DataAnnotations;

namespace Test.Common.Models
{
    public class InitCalculationResponseModel
    {
        [Required]
        public Guid CalculationId { get; set; }
    }
}
