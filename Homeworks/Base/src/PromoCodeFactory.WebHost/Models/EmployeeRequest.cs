using System.Collections.Generic;

namespace PromoCodeFactory.WebHost.Models
{
    public class EmployeeRequest
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        public List<RoleItemResponse> Roles { get; set; }

        public int AppliedPromocodesCount { get; set; }
    }
}