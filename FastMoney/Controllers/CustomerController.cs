using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FastMoney.Data;
using FastMoney.Models;
using FastMoney.Models.ViewModels;
using FastMoney.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastMoney.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CustomerController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationLoggedUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (!applicationLoggedUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Status");
            }

            return View();
        }

        public async Task<IActionResult> GetAllCustomer()
        {
            var userList = await _db.ApplicationUser.Where(u => u.IsAdmin == false).ToListAsync();
            return Json(new { data = userList });
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var applicationLoggedUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();
                
                if(applicationLoggedUser==null)
                {
                    return NotFound();
                }

                id = applicationLoggedUser.Id;
            }

            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost, ActionName("Details")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DetailsConfirmed(string? id)
        {
            var applicationUserFromDb = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (id == null)
            {
                return NotFound();
            }

            if (applicationUserFromDb == null)
            {
                return NotFound();
            }

            if (applicationUserFromDb.LockoutEnd == null || applicationUserFromDb.LockoutEnd < DateTime.Now)
            {
                applicationUserFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            else
            {
                applicationUserFromDb.LockoutEnd = DateTime.Now;
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (applicationUser == null)
            {
                return NotFound();
            }

            return View(applicationUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                var user = await _db.ApplicationUser.Where(m => m.Id == applicationUser.Id).FirstOrDefaultAsync();

                user.Name = applicationUser.Name;
                user.IdentificationNumber = applicationUser.IdentificationNumber;
                user.DateOfBirth = applicationUser.DateOfBirth;
                user.StreetAddress = applicationUser.StreetAddress;
                user.City = applicationUser.City;
                user.State = applicationUser.State;
                user.ZipCode = applicationUser.ZipCode;
                user.PhoneNumber = applicationUser.PhoneNumber;
                user.IsAdmin = applicationUser.IsAdmin;

                _db.Update(user);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(applicationUser);

        }

        public async Task<IActionResult> Deposit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var accountUser = await _db.ApplicationUser.Where(m => m.Id == id).FirstOrDefaultAsync();

            if (accountUser == null)
            {
                return NotFound();
            }

            TransactionViewModel transactionView = new TransactionViewModel
            {
                ApplicationUser = accountUser,
                Transaction = new Transaction()
            };

            return View(transactionView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deposit(TransactionViewModel transactionView)
        {
            if (ModelState.IsValid)
            {
                var transactionList = await _db.Transaction.ToListAsync();
                var transactionCount= transactionList.Count + 1;
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var transactionNumber = unixTimestamp + "" + transactionCount;
               
                transactionView.Transaction.DateOfTransaction = DateTime.Now.Date;
                transactionView.Transaction.Particulars = SD.Deposited;
                transactionView.Transaction.TransactionStatus = SD.TransactionSuccessful;
                transactionView.Transaction.AccountId = transactionView.ApplicationUser.Id;
                transactionView.Transaction.TransactionNumber =Convert.ToInt64(transactionNumber);
                _db.Transaction.Add(transactionView.Transaction);
                await _db.SaveChangesAsync();

                var accountUser= await _db.ApplicationUser.Where(m => m.Id == transactionView.ApplicationUser.Id).FirstOrDefaultAsync();
                var newBalance = (accountUser.CurrentBalance + transactionView.Transaction.Amount);
                accountUser.CurrentBalance = newBalance;
                _db.Update(accountUser);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(transactionView);
        }
    }
}