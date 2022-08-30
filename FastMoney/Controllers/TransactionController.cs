using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using FastMoney.Data;
using FastMoney.Models.ViewModels;
using FastMoney.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FastMoney.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        public TransactionController(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
        }
        public IActionResult Index()
        {
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

        public async Task<IActionResult> FundTransfer()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var accountUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            TransactionViewModel transactionView = new TransactionViewModel
            {
                ApplicationUser = accountUser,
                Transaction = null
            };

            return View(transactionView);
        }

        public async Task<IActionResult> SearchBeneficiary(int? accountNumber)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.AccountNumber != accountNumber)
            {
                var beneficiaryUser = await _db.ApplicationUser.Where(m => m.AccountNumber == accountNumber).ToListAsync();
                return Json(beneficiaryUser);
            }

            return Json("not found");
        }

        public async Task<IActionResult> GenerateOtp(int? id)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (!applicationUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Status");
            }

            int otpValue = new Random().Next(100000, 999999);
            
            
            var transactionFromDatabase=await _db.Transaction.Where(t=>t.Id==id).Include(s => s.ApplicationUser).FirstOrDefaultAsync();

            if (transactionFromDatabase == null)
            {
                return NotFound();
            }

            string message = "Transaction Number: "+transactionFromDatabase.TransactionNumber+". Your OTP Number is " + otpValue + " ( Sent By : Valuation Ascension Bank)";

            transactionFromDatabase.TransactionOtp = otpValue;
            _db.Update(transactionFromDatabase);
            await _db.SaveChangesAsync();

            await _emailSender.SendEmailAsync(transactionFromDatabase.ApplicationUser.Email, "Verify Your Transaction By Entering The OTP", message);

            return RedirectToAction("OtpGenerated", "Status");
        }

        public async Task<IActionResult> VerifyOtp(int? id)
        {
            //string actualOtp = HttpContext.Session.GetString("CurrentOTP");

            //if (otpValue == null)
            //{
            //    return Problem("error");
            //}

            //if(otpValue==actualOtp)
            //{
            //    return Json("success");
            //}

            //else
            //{
            //    return Problem("error");
            //}

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Status");
            }

            else
            {
                var transactionFromDb = await _db.Transaction.Where(u => u.Id ==id).Include(s => s.ApplicationUser).FirstOrDefaultAsync();
                TransactionViewModel transactionViewModel = new TransactionViewModel()
                {
                    Transaction=transactionFromDb,
                    ApplicationUser= applicationUser
                };
                return View(transactionViewModel);
            }

        }

        public async Task<IActionResult> VerifyOtpByCustomer(int? id,int? otp)
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Status");
            }

            
            var transactionFromDb = await _db.Transaction.Where(u => u.Id == id).Include(s => s.ApplicationUser).FirstOrDefaultAsync();

            if (otp != transactionFromDb.TransactionOtp)
            {
                return Problem("error");
            }


            if (transactionFromDb.TransactionStatus != SD.TransactionPending)
            {
                return Problem("error");
            }

            transactionFromDb.TransactionStatus = SD.TransactionSuccessful;
            _db.Update(transactionFromDb);
            await _db.SaveChangesAsync();

            return Json("success");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FundTransfer(TransactionViewModel transactionView)
        {
            if (ModelState.IsValid)
            {
               
                var accountUser = await _db.ApplicationUser.Where(m => m.Id == transactionView.ApplicationUser.Id).FirstOrDefaultAsync();

                if(accountUser.CurrentBalance<transactionView.Transaction.Amount)
                {
                    ViewBag.Error = "You don't have sufficient balance";
                    return View(transactionView);
                }

                var transactionList = await _db.Transaction.ToListAsync();
                var transactionCount = transactionList.Count + 1;
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                var transactionNumber = unixTimestamp + "" + transactionCount;

               
                var newBalance = (accountUser.CurrentBalance - transactionView.Transaction.Amount);
                accountUser.CurrentBalance = newBalance;
                _db.Update(accountUser);
                await _db.SaveChangesAsync();


                transactionView.Transaction.DateOfTransaction = DateTime.Now.Date;
                transactionView.Transaction.Particulars = SD.Transfered;
                transactionView.Transaction.AccountId = transactionView.ApplicationUser.Id;
                transactionView.Transaction.TransactionStatus = SD.TransactionPending;
                transactionView.Transaction.TransactionNumber =Convert.ToInt64(transactionNumber);
                _db.Transaction.Add(transactionView.Transaction);
                await _db.SaveChangesAsync();

                //var beneficiaryUser = await _db.ApplicationUser.Where(m => m.Id == transactionView.BeneficiaryUser.Id).FirstOrDefaultAsync();
                //var beneficiaryNewBalance = (beneficiaryUser.CurrentBalance + transactionView.Transaction.Amount);
                //beneficiaryUser.CurrentBalance = beneficiaryNewBalance;
                //_db.Update(beneficiaryUser);
                //await _db.SaveChangesAsync();

                //Models.Transaction beneficiaryTransaction = new Models.Transaction();

                //beneficiaryTransaction.DateOfTransaction = DateTime.Now.Date;
                //beneficiaryTransaction.Particulars = "Fund Transfer Received";
                //beneficiaryTransaction.AccountId = transactionView.BeneficiaryUser.Id;
                //beneficiaryTransaction.BeneficiaryId = transactionView.ApplicationUser.Id;
                //beneficiaryTransaction.Amount = transactionView.Transaction.Amount;
                //beneficiaryTransaction.Remarks = transactionView.Transaction.Remarks;
                //_db.Transaction.Add(beneficiaryTransaction);
                //await _db.SaveChangesAsync();


                return RedirectToAction(nameof(TransactionSubmitted));
            }

            return View(transactionView);
        }

        public IActionResult TransactionSubmitted()
        {
            return View();
        }


        public async Task<IActionResult> GetAllTransactionForAdmin()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.IsAdmin)
            {
                return View();
            }

            else
            {
                return RedirectToAction("AccessDenied", "Status");
            }
        }

        public async Task<IActionResult> GetAllTransactionForCustomer()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (!applicationUser.IsAdmin)
            {
                return View();
            }

            else
            {
                return RedirectToAction("AccessDenied", "Status");
            }
        }


        public async Task<IActionResult> GetAllPendingTransactionForAdmin()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.IsAdmin)
            {
                var transactionList = await _db.Transaction.Where(u => u.TransactionStatus == SD.TransactionPending).Include(s => s.ApplicationUser).ToListAsync();
                return Json(new { data = transactionList });
            }

            else
            {
                return RedirectToAction("AccessDenied", "Status");
            }
        }

        

        public async Task<IActionResult> GetAllPendingTransactionForCustomer()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var applicationUser = await _db.ApplicationUser.Where(m => m.Id == claim.Value).FirstOrDefaultAsync();

            if (applicationUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Status");
            }

            else
            {
                var transactionList = await _db.Transaction.Where(u => u.AccountId == applicationUser.Id && u.TransactionStatus == SD.TransactionPending).Include(s => s.ApplicationUser).ToListAsync();
                return Json(new { data = transactionList });
            }
        }


    }
}