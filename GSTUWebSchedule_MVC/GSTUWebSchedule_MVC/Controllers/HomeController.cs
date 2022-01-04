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
            if (model.Case == 1)
            {
                model.DbTable = dbTable.DbTable.Where(m => m.Username == User.Identity.Name);
                //CASE 1
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
            } else
            {
                //CASE 2
                if (model.Case >= 1 && model.Case <= 2 && model.Subject >= 0 && model.Subject < Subjects.size && model.Group.Length >= 4 && model.Students.Length >= 1)
                {
                    string group = model.Group, students = model.Students;
                    int subject = model.Subject;
                    model.DbTable = dbTable.DbTable.Where(m => m.Username == User.Identity.Name);

                    if (model.DbTable.Count(m => m.Subject == subject && m.Group == group) == 0)
                    {
                        model.Error2 = "error";
                        ModelState.AddModelError("", "Такой группы нет!");
                        return View(model);
                    }
                    var thismodel = model.DbTable.Where(m => m.Subject == subject && m.Group == group).ToArray()[0];

                    var splited = students.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var Old = new string[thismodel.Students.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length];
                    int counter = 0;
                    List<string> New = new List<string>();

                    for (int i = 0; i < splited.Length; i++)
                        if (splited[i].Split('_', StringSplitOptions.RemoveEmptyEntries)[0] == "0")
                            Old[counter++] = splited[i].Split('_', StringSplitOptions.RemoveEmptyEntries)[1];
                        else
                            New.Add(splited[i].Split('_', StringSplitOptions.RemoveEmptyEntries)[1]);

                    if (counter > thismodel.Students.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length || Old.Length - 1 > thismodel.Students.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length)
                    {
                        model.Error2 = "error";
                        ModelState.AddModelError("", "Такая таблица уже есть!");
                        return View(model);
                    }

                    List<KeyValuePair<string, int> > sorted = new List<KeyValuePair<string, int> >();
                    var splitedLabs = thismodel.Labs.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var splitedReports = thismodel.Reports.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    var splitedDefences = thismodel.Defences.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < counter; i++)
                        if (Old[i] != "-1") sorted.Add(new KeyValuePair<string, int>(Encoding.UTF8.GetString(Convert.FromBase64String(Old[i])), i));
                    foreach (var n in New)
                        if (n != "-1") sorted.Add(new KeyValuePair<string, int>(Encoding.UTF8.GetString(Convert.FromBase64String(n)), int.MaxValue));
                    sorted = sorted.OrderBy(obj => obj.Key).ToList();

                    string zeroString = "";
                    for (int j = 0; j < thismodel.NumberOfLabs; j++) zeroString += "0";

                    string studentsResult = "", labsResult = "", reportsResult = "", defencesResult = "";
                    foreach (var n in sorted)
                    {
                        if (n.Value == int.MaxValue)
                        {
                            labsResult += zeroString + " ";
                            reportsResult += zeroString + " ";
                            defencesResult += zeroString + " ";
                        } else
                        {
                            labsResult += splitedLabs[n.Value] + " ";
                            reportsResult += splitedReports[n.Value] + " ";
                            defencesResult += splitedDefences[n.Value] + " ";
                        }
                        studentsResult += Convert.ToBase64String(Encoding.UTF8.GetBytes(n.Key)) + " ";
                    }
                    labsResult = labsResult.Remove(labsResult.Length - 1);
                    reportsResult = reportsResult.Remove(reportsResult.Length - 1);
                    defencesResult = defencesResult.Remove(defencesResult.Length - 1);
                    studentsResult = studentsResult.Remove(studentsResult.Length - 1);

                    thismodel.Labs = labsResult;
                    thismodel.Reports = reportsResult;
                    thismodel.Defences = defencesResult;
                    thismodel.Students = studentsResult;

                    dbTable.DbTable.Update(thismodel);
                    await dbTable.SaveChangesAsync();
                    model.Error2 = "ok";
                }
                else model.Error2 = "error";
                return View(model);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
