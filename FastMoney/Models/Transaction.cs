using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FastMoney.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        [DataType(DataType.Date)]
        public DateTime DateOfTransaction { get; set; }

        [Display(Name = "Account")]
        public string AccountId { get; set; }

        [ForeignKey("AccountId")]
        public virtual ApplicationUser ApplicationUser { get; set; }
        public string Particulars { get; set; }
        [Display(Name = "Transfer Amount")]
        public double Amount { get; set; }
        public string Remarks { get; set; }

        [Display(Name = "Beneficiary Full Name")]
        public string BeneficiaryName { get; set; }

        [Display(Name = "Beneficiary Account Number")]
        public string BeneficiaryAccountNumber { get; set; }

        [Display(Name = "Beneficiary Bank Name")]
        public string BeneficiaryBankName { get; set; }
        [Display(Name = "Beneficiary Phone Number")]
        public string BeneficiaryPhoneNumber { get; set; }
        [EmailAddress]
        [Display(Name = "Beneficiary Email")]
        public string BeneficiaryEmail { get; set; }
        public int TransactionOtp { get; set; }
        public string TransactionStatus { get; set; }
        public long TransactionNumber { get; set; }

    }
}
