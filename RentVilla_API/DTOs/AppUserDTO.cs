namespace RentVilla_API.DTOs
{
    public class AppUserDTO
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public DateTime Created { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public short ZipCode { get; set; }

        public string State { get; set; }

        public string City { get; set; }
    }
}
