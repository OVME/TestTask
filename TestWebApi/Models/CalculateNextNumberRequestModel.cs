using System;
using System.ComponentModel.DataAnnotations;

namespace TestWebApi.Models
{
    public class CalculateNextNumberRequestModel
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int CurrentNumber { get; set; }
    }
}