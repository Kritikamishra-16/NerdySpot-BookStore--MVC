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
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string ISBN { get; set; }
        //"International Standard Book Number."

        [Required]
        public string Author { get; set; }

        [Required]
        [Display(Name = "List Price")]
        [Range(1,1000)]
        public double ListPrice { get; set; }

        [Required]
        [Display(Name = "Price 1-50")]
        [Range(1, 1000)]
        public double Price { get; set; }

        [Required]
        [Display(Name = "Price 50+")]
        [Range(1, 1000)]
        public double Price50 { get; set; }

        [Required]
        [Display(Name = "Price 100+")]
        [Range(1, 1000)]
        public double Price100 { get; set; }

        /*
         * the code snippet suggests that the class contains a property CategoryId representing the foreign key column 
         * in the database. The [ForeignKey("CategoryId")] attribute indicates that CategoryId is a foreign key linking 
         * this entity to the Category entity. Additionally, the Category property acts as a navigation property, allowing 
         * you to access the related Category entity associated with the foreign key CategoryId. This is a common way 
         * to model relationships between entities in Entity Framework Code First approach.
         */

        //this Product table has a forign key CategoryId
        public int CategoryId { get; set; }
        //Naviagation property to category table
        [ForeignKey("CategoryId")] //this Category property is used for foreign key navigation or CategoryId 
        [ValidateNever]
        public Category Category { get; set; }


        //IMAGE URL
        [ValidateNever]
        public string ImageUrl { get; set; }

    }
}
