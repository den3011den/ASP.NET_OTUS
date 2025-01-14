using System.Collections.Generic;

namespace PromoCodeFactory.WebHost.Models
{
    public class ErrorResponse
    {

        public string ErrorMessage { get; set; }

        public IEnumerable<EmployeeShortResponse> Employees { get; set; }

    }
}