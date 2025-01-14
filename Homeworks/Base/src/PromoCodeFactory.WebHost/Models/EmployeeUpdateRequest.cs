using System;
using System.Collections.Generic;

namespace PromoCodeFactory.WebHost.Models
{
    public class EmployeeUpdateRequest
    {
        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        //public string FullName => $"{FirstName} {LastName}";

        public string Email { get; set; }

        public List<Guid> Roles { get; set; }

        public int AppliedPromocodesCount { get; set; }
    }
}