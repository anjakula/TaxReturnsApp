using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TaxReturns.DAL;
using TaxReturns.Models;
using TaxReturns.Helpers;
using System;
using System.Globalization;

namespace TaxReturns.Controllers
{
    public class AccountTransactionsController : Controller
    {
        private TaxReturnContext db = new TaxReturnContext();

        // GET: AccountTransactions
        public ActionResult Index(string filter)
        {
            IEnumerable<AccountTransaction> acs = db.accountTransactions.ToList();
            if (!string.IsNullOrEmpty(filter))
            {
                acs = acs.Where(x => x.Account.Contains(filter) || x.Description.Contains(filter));

            }
            return View(acs);
        }



        // GET: AccountTransactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountTransaction accountTransaction = db.accountTransactions.Find(id);
            if (accountTransaction == null)
            {
                return HttpNotFound();
            }
            return View(accountTransaction);
        }

        // POST: AccountTransactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Account,Description,CurrencyCode,Amount")] AccountTransaction accountTransaction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var results = Utility.CultureInfoFromCurrencyISO(accountTransaction.CurrencyCode);
                    if (results.Count>0)
                    {
                        db.Entry(accountTransaction).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Unable to save changes. Currency Code Not valid.");

                    }
                }
            }
            catch (DataException dex)
            {
                //Log the error (add a variable name after DataException)
              //  Elmah.ErrorSignal.FromCurrentContext().Raise(dex);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(accountTransaction);
        }

        // GET: AccountTransactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountTransaction accountTransaction = db.accountTransactions.Find(id);
            if (accountTransaction == null)
            {
                return HttpNotFound();
            }
            return View(accountTransaction);
        }

        // POST: AccountTransactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AccountTransaction accountTransaction = db.accountTransactions.Find(id);
            db.accountTransactions.Remove(accountTransaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // GET: AccountTransactions/Create

        public ActionResult Create()
        {
            return View();
        }


        // POST: AccountTransactions/Create

        [HttpPost]
        [ActionName("Create")]
        public ActionResult Upload()
        {
            if (Request.Files["FileUpload1"].ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();

                HttpPostedFileBase postedFile = Request.Files["FileUpload1"];
                string postedFileName = Path.GetFileName(postedFile.FileName);

                string connString = "";

                string[] validFileTypes = { ".xls", ".xlsx", ".csv" };

                string path1 = string.Format("{0}/{1}", Server.MapPath("~/Content/Uploads"), postedFileName);
                if (!Directory.Exists(path1))
                {
                    Directory.CreateDirectory(Server.MapPath("~/Content/Uploads"));
                }
                if (validFileTypes.Contains(extension))
                {
                    // create errors data table
                    var errorsdt = new System.Data.DataTable("ErrorsDataTable");
                    errorsdt.Columns.Add("Account", typeof(string));
                    errorsdt.Columns.Add("Description", typeof(string));
                    errorsdt.Columns.Add("CurrencyCode", typeof(String));
                    errorsdt.Columns.Add("Amount", typeof(decimal));

                    if (System.IO.File.Exists(path1))
                    { System.IO.File.Delete(path1); }
                    Request.Files["FileUpload1"].SaveAs(path1);
                    if (extension == ".csv")
                    {
                        DataTable dt = Utility.ConvertCSVtoDataTable(path1);
                        InsertData(dt, errorsdt);

                    }
                    //Connection String to Excel Workbook  
                    //optional
                    else if (extension.Trim() == ".xls")
                    {
                        connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                        DataTable dt = Utility.ConvertXSLXtoDataTable(path1, connString);
                        InsertData(dt, errorsdt);
                    }
                    else if (extension.Trim() == ".xlsx")
                    {
                        connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                        DataTable dt = Utility.ConvertXSLXtoDataTable(path1, connString);
                        InsertData(dt, errorsdt);
                    }

                }
                else
                {
                    ViewBag.Error = "Please Upload Files in .xls, .xlsx or .csv format";

                }

            }

            return View();
        }

        private void InsertData(DataTable dt, DataTable errorsdt)
        {
            int count = 0;

            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    AccountTransaction at = new AccountTransaction();

                    try
                    {
                        at.Account = dt.Rows[i]["Account"].ToString().Trim();
                        at.Description = dt.Rows[i]["Description"].ToString().Trim();
                        at.CurrencyCode = dt.Rows[i]["CurrencyCode"].ToString().Trim();
                        at.Amount = Convert.ToDecimal(dt.Rows[i]["Amount"].ToString().Trim());
                        if (ModelState.IsValid)
                        {
                            //Get currency symbol by currency code
                            var results = Utility.CultureInfoFromCurrencyISO(at.CurrencyCode);
                            if (results.Count > 0)
                            {
                                db.accountTransactions.Add(at);
                                count = count + 1;
                                db.SaveChanges();

                            }
                        }
                        else
                        {
                            // insert row values
                            errorsdt.Rows.Add(new Object[] { at.Account, at.Description, at.CurrencyCode, at.Amount });

                        }

                    }
                    catch (Exception e)
                    {
                        // insert row values
                        errorsdt.Rows.Add(new Object[] { at.Account, at.Description, at.CurrencyCode, at.Amount });


                    }
                }
                ViewBag.Data = errorsdt;
                if (count > 0)
                {
                    ViewBag.Count = count + " records inserted successfully!";
                }

            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
