using RentVilla_API.Helpers;
using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.Entities
{
    public class AppUserAddress
    {
        // 1 to Many (1 User can have many addresses)
        [Required]
        public int Id { get; set; }

        [Required]
        public int AppuserID { get; set; }

        [Required]
        public AppUser AppUser { get; set; }
        [Required]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public DateTime? Created { get; set; }

        [Required]
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        [Required]
        public short? ZipCode { get; set; }

        [Required]
        public string State { get; set; }

        [Required]
        public string City { get; set; }

        public bool ShouldSerializeErrorMessages() => JsonSerialization.ShouldSerializeProperty(this, nameof(AddressLine2));
    }
}