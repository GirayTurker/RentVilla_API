using System.ComponentModel.DataAnnotations;

namespace RentVilla_API.DTOs
{
    public class AppUserAddressDTO
    {
        [Required]
        public int AppuserID { get; set; }

        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public short? ZipCode { get; set; }

        public string State { get; set; }

        public string City { get; set; }
    }
}
