using GSTUWebSchedule_MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Controllers
{
    public class HomeController : Controller
    {
        private DbTableContext dbTable;
        private readonly ILogger<HomeController> _logger;

        public HomeController(DbTableContext context)
        {
            dbTable = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Index()
        {
            IndexModel model = new IndexModel();
            model.DbTable = dbTable.DbTable.Where(m => m.Username == User.Identity.Name);
            int subject = -1;
            if (!string.IsNullOrEmpty(HttpContext.Request.Query["subject"]) && int.TryParse(HttpContext.Request.Query["subject"], out subject) && subject >= 0 && subject < Subjects.size && !string.IsNullOrEmpty(HttpContext.Request.Query["group"]))
            {
                model.Case = true;
                if (model.DbTable.Count(m => m.Subject == subject && m.Group == HttpContext.Request.Query["group"]) == 0) model.Case = false; 
                else
                    model.DbTable = model.DbTable.Where(m => m.Subject == subject && m.Group == HttpContext.Request.Query["group"]);
            }
            else model.Case = false;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(SaveTableModel model)
        {
            DbTableModel outDbModel = null;
            if (CheckIfValid(model, out outDbModel))
            {
                outDbModel.Labs = model.dataLabs;
                outDbModel.Reports = model.dataReports;
                outDbModel.Defences = model.dataDefences;
                dbTable.DbTable.Update(outDbModel);
                await dbTable.SaveChangesAsync();

                return LocalRedirect($"/Home/Index/?subject={model.dataSubject}&group={model.dataGroup}");
            }
            return RedirectPermanent("/Home/Index");
        }
        public bool CheckIfValid(SaveTableModel model, out DbTableModel outDbModel)
        {
            outDbModel = null;

            if (model == null) return false;
            if (model.dataSubject < 0 || model.dataSubject >= Subjects.size || string.IsNullOrEmpty(model.dataGroup)) return false;
            outDbModel = dbTable.DbTable.Where(m => m.Username == User.Identity.Name && m.Subject == model.dataSubject && m.Group == model.dataGroup).ToArray()[0];
            if (outDbModel == null) return false;
            int numberOfStudents = outDbModel.Students.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            var a = model.dataLabs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (a.Length != numberOfStudents) return false;
            foreach (string temp in a)
            {
                if (temp.Length != outDbModel.NumberOfLabs) return false;
                for (int i = 0; i < temp.Length; i++) if (temp[i] != '1' && temp[i] != '0') return false;
            }

            var b = model.dataReports.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (b.Length != numberOfStudents) return false;
            foreach (string temp in b)
            {
                if (temp.Length != outDbModel.NumberOfLabs) return false;
                for (int i = 0; i < temp.Length; i++) if (temp[i] != '1' && temp[i] != '0') return false;
            }

            var c = model.dataDefences.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (c.Length != numberOfStudents) return false;
            foreach (string temp in c)
            {
                if (temp.Length != outDbModel.NumberOfLabs) return false;
                for (int i = 0; i < temp.Length; i++) if (temp[i] != '1' && temp[i] != '0') return false;
            }

            return true;
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTable(DeleteTableModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.Confirm1 == 1 && model.Confirm2 == 1)
                {
                    var outDbModel = dbTable.DbTable.Where(m => m.Username == User.Identity.Name && m.Subject == model.Subject && m.Group == model.Group).ToArray()[0];
                    dbTable.DbTable.Remove(outDbModel);
                    await dbTable.SaveChangesAsync();
                }
            }
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Manage()
        {
            var thisUserTable = dbTable.DbTable.Where(m => m.Username == User.Identity.Name);
            ManageTableModel model = new ManageTableModel() { DbTable = thisUserTable };
            return View(model);
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageTableModel model)
        {
            model.DbTable = dbTable.DbTable.Where(m => m.Username == User.Identity.Name);
            if (ModelState.IsValid)
            {
                string zeroString = "";
                var splited = model.Students.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var decodedSplited = splited;
                for (int i = 0; i < decodedSplited.Length; i++) decodedSplited[i] = Encoding.UTF8.GetString(Convert.FromBase64String(splited[i]));
                Array.Sort(decodedSplited);
                for (int i = 0; i < decodedSplited.Length; i++) decodedSplited[i] = Convert.ToBase64String(Encoding.UTF8.GetBytes(decodedSplited[i]));
                string sortedStudents = "";
                for (int i = 0; i < decodedSplited.Length - 1; i++) sortedStudents += decodedSplited[i] + " "; sortedStudents += decodedSplited[decodedSplited.Length - 1];
                model.Students = sortedStudents;

                for (int i = 0; i < splited.Length; i++)
                {
                    for (int j = 0; j < model.NumberOfLabs; j++) zeroString += "0";
                    zeroString += " ";
                }
                zeroString = zeroString.Remove(zeroString.Length - 1);

                string semestr = "", group = "";
                if (model.Semester == 1) semestr = "I sem."; else if (model.Semester == 2) semestr = "II sem."; else semestr = "I-II sem.";
                group = model.Group + " (" + model.Year + ") " + semestr;

                var cell = await dbTable.DbTable.FirstOrDefaultAsync(u => u.Username == User.Identity.Name && u.Group == group && u.Subject == model.Subject);
                if (cell == null)
                {
                    dbTable.DbTable.Add(new DbTableModel
                    {
                        Username = User.Identity.Name,
                        Subject = model.Subject,
                        Group = group,
                        Students = model.Students,
                        NumberOfLabs = model.NumberOfLabs,
                        Labs = zeroString,
                        Reports = zeroString,
                        Defences = zeroString
                    });
                    await dbTable.SaveChangesAsync();

                    model.Error = "ok";
                }
                else
                {
                    model.Error = "error";
                    ModelState.AddModelError("", "Такая таблица уже есть!");
                }
            }
            else model.Error = "error";
            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
