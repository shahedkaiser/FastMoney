using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastMoney.Models.ViewModels
{
    public class TransactionViewModel
    {
        public Transaction Transaction { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
