using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastMoney.Data;
using FastMoney.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastMoney.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if(applicationUser.IsAdmin)
            {
                HttpContext.Session.SetString("userRole", "admin");
                var transactionList = await _db.Transaction.ToListAsync();
                ViewBag.TransactionCount = transactionList.Count;
                var customers = await _db.ApplicationUser.Where(m => m.IsAdmin == false).ToListAsync();
                //ViewBag.TotalCustomer = customers.Count;
                var pendingTransactionList = await _db.Transaction.Where(u =>u.TransactionStatus == SD.TransactionPending).ToListAsync();
                ViewBag.PendingTransactionCount = pendingTransactionList.Count;
                var successTransactionList = await _db.Transaction.Where(u =>u.TransactionStatus == SD.TransactionSuccessful).ToListAsync();
                ViewBag.SuccessTransactionCount = successTransactionList.Count;
                var sumOfCurrentBalance = customers.Sum(x => x.CurrentBalance).ToString();
                ViewBag.CurrentBalance = sumOfCurrentBalance;
                var depositTransactionList = await _db.Transaction.Where(u=>u.Particulars==SD.Deposited).ToListAsync();
                var sumOfDeposit = depositTransactionList.Sum(x => x.Amount).ToString();
                ViewBag.TotalDeposit = sumOfDeposit;
                var transferTransactionList = await _db.Transaction.Where(u => u.Particulars == SD.Transfered).ToListAsync();
                var sumOfTransfer = transferTransactionList.Sum(x => x.Amount).ToString();
                ViewBag.TotalTransfer = sumOfTransfer;
            }

            else
            {
                HttpContext.Session.SetString("userRole", "customer");
                ViewBag.CurrentBalance = applicationUser.CurrentBalance;
                var transactionList = await _db.Transaction.Where(u => u.AccountId == applicationUser.Id).ToListAsync();
                ViewBag.TransactionCount = transactionList.Count;
                var pendingTransactionList = await _db.Transaction.Where(u => u.AccountId == applicationUser.Id && u.TransactionStatus==SD.TransactionPending).ToListAsync();
                ViewBag.PendingTransactionCount = pendingTransactionList.Count;
                var successTransactionList = await _db.Transaction.Where(u => u.AccountId == applicationUser.Id && u.TransactionStatus == SD.TransactionSuccessful).ToListAsync();
                ViewBag.SuccessTransactionCount = successTransactionList.Count;
                var depositTransactionList = await _db.Transaction.Where(u =>u.ApplicationUser.Id== applicationUser.Id && u.Particulars == SD.Deposited).ToListAsync();
                var sumOfDeposit = depositTransactionList.Sum(x => x.Amount).ToString();
                ViewBag.TotalDeposit = sumOfDeposit;
                var transferTransactionList = await _db.Transaction.Where(u =>u.ApplicationUser.Id==applicationUser.Id && u.Particulars == SD.Transfered).ToListAsync();
                var sumOfTransfer = transferTransactionList.Sum(x => x.Amount).ToString();
                ViewBag.TotalTransfer = sumOfTransfer;
            }

            return View();
        }

        public async Task<IActionResult> GetAllTransaction()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if(applicationUser.IsAdmin)
            {
                var transactionList = await _db.Transaction.Include(s => s.ApplicationUser).ToListAsync();
                return Json(new { data = transactionList });
            }

            else
            {
                var transactionList = await _db.Transaction.Where(u=>u.AccountId==applicationUser.Id).Include(s => s.ApplicationUser).ToListAsync();
                return Json(new { data = transactionList });
            }

            
        }


    }
}