using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdySpot.Models
{
    //ApplicationUser has all the default settings of identityUser
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string? StreetAddress { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? PostalCode { get; set;}

        //NULLABLE bcz if the user is customer user then he do not have a company 
        //but if it is a company user only then it will ave a CompanyId
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")] //CompanyId is the navigation property this Company
        [ValidateNever]
        public Company Company { get; set; }


    }
}
