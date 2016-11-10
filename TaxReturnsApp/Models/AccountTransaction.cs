using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;


namespace TaxReturns.Models
{
    public class AccountTransaction
    {
        public int Id { get; set; } // Id (Primary key)
        [Required(ErrorMessage = "Required")]
        public string Account { get; set; } // Account (length: 2147483647)
        [Required(ErrorMessage = "Required")]
        public string Description { get; set; } // Description (length: 2147483647)

        [Required(ErrorMessage = "Required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string CurrencyCode { get; set; } // CurrencyCode (length: 3)
        [Required(ErrorMessage = "Required")]
        [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Invalid price")]
       
        public decimal Amount { get; set; } // Amount
    }


}