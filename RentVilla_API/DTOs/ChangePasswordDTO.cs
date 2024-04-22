using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.DTOs
{
    public class ChangePasswordDTO
    {
        public string Password { get; set; }


        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }
    }
}
