using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FastMoney.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string IdentificationNumber { get; set; }
        public double CurrentBalance { get; set; }
        public bool IsAdmin { get; set; }
        public long AccountNumber { get; set; }

        [DataType(DataType.Date)]
        public DateTime OpeningDate { get; set; }
    }
}
