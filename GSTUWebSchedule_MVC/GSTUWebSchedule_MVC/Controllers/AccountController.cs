using GSTUWebSchedule_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using UAParser;

namespace GSTUWebSchedule_MVC.Controllers
{
    public class AccountController : Controller
    {
        private DbUsersContext dbUsers;
        private LastVisitsContext lastVisits;
        private DbEmailCodesContext dbCodes;
        private IWebHostEnvironment webHostEnvironment;
        public AccountController(DbUsersContext context, LastVisitsContext context2, DbEmailCodesContext context3, IWebHostEnvironment _webHostEnvironment)
        {
            dbUsers = context;
            lastVisits = context2;
            dbCodes = context3;
            webHostEnvironment = _webHostEnvironment;
        }

        public void Log(string x, string UserName = null)
        {
            var logPath = Path.Combine(webHostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
            if (UserName == null) UserName = User.Identity.Name;
            var thisUserDirectory = Path.Combine(logPath, UserName);
            if (!Directory.Exists(thisUserDirectory)) Directory.CreateDirectory(thisUserDirectory);

            try
            {
                System.IO.File.AppendAllText(Path.Combine(thisUserDirectory, DateTime.Now.ToString("dd.MM.yyyy") + ".log"),
                    "[" + DateTime.Now + "] " + x + Environment.NewLine);
            } catch { }
        }

        public static Regex MobileCheck = new Regex(@"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        public static Regex MobileVersionCheck = new Regex(@"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-", RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        public static bool fBrowserIsMobile(string u)
        {
            if (u.Length < 4) return false;
            if (MobileCheck.IsMatch(u) || MobileVersionCheck.IsMatch(u.Substring(0, 4))) return true;

            return false;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            model.Approved = true;
            if (ModelState.IsValid)
            {
                DbUsersModel user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == model.Username);

                /*if (await dbUsers.DbUsers.FirstOrDefaultAsync(m => m.Username == "root") == null)
                {
                    var a = new DbUsersModel
                    {
                        id = 1,
                        Username = "root",
                        Password = "N3A4C4qh/TATid7PqMisEAgJUt2NxLxMMYHyFitvM4E=",
                        Salt = "s02X2Yop9dDpDmtv8WjQyA==",
                        Email = "root@edu.gstu.by",
                        EmailConfirmed = true,
                        LastEmailConfirmDate = DateTime.Parse("01.01.2022 1:00:53"),
                        Name = "Root",
                        Surname = "Root",
                        Middlename = "Toor",
                        RegisterTime = DateTime.Parse("01.01.2022 1:00:53"),
                        Language = "ru",
                        Approved = "true",
                        Role = "Ranged-Creep"
                    };
                    dbUsers.DbUsers.Add(a);
                    await dbUsers.SaveChangesAsync();
                }*/

                string hashed = "";
                if (user != null)
                {
                    //Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                    hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: model.Password,
                        salt: Convert.FromBase64String(user.Salt),
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8));
                } else
                {
                    ModelState.AddModelError("", "Некорректные логин или пароль!");
                    return View(model);
                }

                user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == hashed && u.Approved == "false");
                if (user != null)
                {
                    ModelState.AddModelError("", "Аккаунт ещё не активирован");
                    model.Approved = false;
                    return View(model);
                }

                user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == hashed);
                if (user != null)
                {
                    await Authenticate(model.Username); //Аутентификация

                    var visits = lastVisits.DbTable.Where(m => m.Username == model.Username);
                    if (visits.Count(m => m.Username == model.Username) >= 10)
                    {
                        var min = visits.Min(m => m.Date);
                        for (int i = 0; i < visits.Count(m => m.Username == model.Username) - 9; i++) lastVisits.DbTable.Remove(visits.Where(m => m.Date == min).ToArray()[i]);
                    }

                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    var currentvisit = new LastVisitsModel() { Username = model.Username, Date = DateTime.Now, FullUserAgent = c.String, OS = c.OS.ToString(), Browser = c.UA.ToString(), IP = Request.HttpContext.Connection.RemoteIpAddress.ToString(), isMobile = fBrowserIsMobile(c.String) };
                    lastVisits.Add(currentvisit);

                    await lastVisits.SaveChangesAsync();

                    Log("Login was initiated! " + $"Username: {currentvisit.Username}, Date: {currentvisit.Date}, FullUserAgent: {currentvisit.FullUserAgent}, OS: {currentvisit.OS}, Browser: {currentvisit.Browser}, IP: {currentvisit.IP}, isMobile: {currentvisit.isMobile}", currentvisit.Username);

                    CheckLogs(currentvisit.Username);

                    TempData[currentvisit.Username + "_Role"] = user.Role;

                    CookieOptions option = new CookieOptions();
                    option.Expires = DateTime.Now.AddHours(12);
                    Response.Cookies.Append("lang", user.Language, option);

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Некорректные логин или пароль!");
            }
            return View(model);
        }

        public void CheckLogs(string UserName)
        {
            var logPath = Path.Combine(webHostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
                return;
            }
            var thisUserDirectory = Path.Combine(logPath, UserName);
            if (!Directory.Exists(thisUserDirectory))
            {
                Directory.CreateDirectory(thisUserDirectory);
                return;
            }

            var files = Directory.GetFiles(thisUserDirectory);
            if (files.Length > 14)
            {
                files = files.OrderBy(m => System.IO.File.GetCreationTime(m)).ToArray();
                for (int i = 0; i < files.Length - 14; i++) System.IO.File.Delete(files[i]);
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!Startup.RegistrationOpen) return RedirectToAction("Register", "Account");

            if (ModelState.IsValid)
            {
                DbUsersModel user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);
                if (user == null)
                {
                    //Generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                    byte[] salt = new byte[128 / 8];
                    using (var rngCsp = new RNGCryptoServiceProvider())
                    {
                        rngCsp.GetNonZeroBytes(salt);
                    }
                    //Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                    string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: model.Password,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8));

                    //Adding user to DB
                    var registeruser = new DbUsersModel
                    {
                        Username = model.Username,
                        Password = hashed,
                        Salt = Convert.ToBase64String(salt),
                        Email = model.Email,
                        EmailConfirmed = false,
                        LastEmailConfirmDate = DateTime.Now,
                        Name = model.Name,
                        Surname = model.Surname,
                        Middlename = model.Middlename,
                        RegisterTime = DateTime.Now,
                        Language = "ru",
                        Approved = "false",
                        Role = "Creep"
                    };

                    var codeModel = dbCodes.DbCodes.FirstOrDefault(m => m.Username == registeruser.Username);
                    DateTime oldDate = new DateTime(621355968000100000);
                    if (codeModel != null)
                    {
                        oldDate = codeModel.ConfirmationDate;
                        dbCodes.Remove(codeModel);
                        await dbCodes.SaveChangesAsync();
                    }
                    if (!registeruser.EmailConfirmed && (DateTime.Now - oldDate).TotalHours > 3)
                    {
                        string code = GetHashString(Guid.NewGuid().ToString() + registeruser.Username);
                        codeModel = new EmailCodeModel();
                        codeModel.EmailConfirmationCode = code;
                        codeModel.ConfirmationDate = DateTime.Now;
                        codeModel.Username = registeruser.Username;
                        dbCodes.Add(codeModel);
                        await dbCodes.SaveChangesAsync();

                        await EmailService.SendEmailAsync(registeruser.Language, registeruser.Email, "Confirm your Email", "Welcome",
                        registeruser.Name + " " + registeruser.Middlename,
                        registeruser.Username,
                        "Hello! Confirm your email address to make sure you entered it correctly again.",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + "/Account/ConfirmEmail?code=" + code,
                        "Confirm!",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + Url.Content("~/Home/EntertainmentHeading"));
                    }

                    dbUsers.DbUsers.Add(registeruser);
                    await dbUsers.SaveChangesAsync();

                    model.Error = "ok";

                    Log("Your account has been registered! " + $"Username: {registeruser.Username}, RegisterTime: {registeruser.RegisterTime}", registeruser.Username);
                }
                else
                {
                    ModelState.AddModelError("", "Введённые данные уже заняты!");
                    model.Error = "error";
                }
            }
            else
            {
                ModelState.AddModelError("", "С вашими данными что-то не так!");
                model.Error = "error";
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            Log("Logout was initiated! " + $"CurrentTime: {DateTime.Now}");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Account/Login/?logout=1");
        }

        [Authorize]
        [HttpGet]
        public IActionResult Settings()
        {
            //Adding "default" model
            SettingsModel model = new SettingsModel();
            if (TempData[User.Identity.Name + "_ErrorLogs"] != null) { model.ErrorLogs = TempData[User.Identity.Name + "_ErrorLogs"].ToString(); }
            model.LastVisitsTable = lastVisits.DbTable.Where(m => m.Username == User.Identity.Name).OrderByDescending(m => m.Date);
            model.Email = dbUsers.DbUsers.FirstOrDefault(m => m.Username == User.Identity.Name).Email;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsModel model)
        {
            //Adding "default" model
            model.LastVisitsTable = lastVisits.DbTable.Where(m => m.Username == User.Identity.Name);
            model.Email = dbUsers.DbUsers.FirstOrDefault(m => m.Username == User.Identity.Name).Email;
            if (model.Case != "ChangePassword" && model.Case != "Email")
            {
                model.Error = "error";
                return View();
            }

            if (model.Case == "ChangePassword")
            {
                //CHANGE PASSWORD
                if (ModelState.IsValid && model.NewPassword == model.NewPasswordConfirm)
                {
                    DbUsersModel user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

                    string hashed = "";
                    if (user != null)
                    {
                        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                        hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: model.OldPassword,
                            salt: Convert.FromBase64String(user.Salt),
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 100000,
                            numBytesRequested: 256 / 8));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Не нашлось такого пользователя!");
                        model.Error = "error";
                        return View(model);
                    }

                    if (user.Password != hashed)
                    {
                        ModelState.AddModelError("", "Некорректный пароль!");
                        model.Error = "error";
                        return View(model);
                    }

                    //Generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                    byte[] salt = new byte[128 / 8];
                    using (var rngCsp = new RNGCryptoServiceProvider())
                    {
                        rngCsp.GetNonZeroBytes(salt);
                    }
                    //Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                    hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                        password: model.NewPassword,
                        salt: salt,
                        prf: KeyDerivationPrf.HMACSHA256,
                        iterationCount: 100000,
                        numBytesRequested: 256 / 8));

                    //Updating user back to DB
                    user.Password = hashed;
                    user.Salt = Convert.ToBase64String(salt);
                    dbUsers.Update(user);
                    await dbUsers.SaveChangesAsync();

                    Log("Your password has been changed! " + $"CurrentTime: {DateTime.Now}");

                    await EmailService.SendEmailAsync(user.Language, user.Email, "Your password has been changed!", "Ouch",
                            user.Name + " " + user.Middlename,
                            user.Username,
                            "Hello! Your account password has been changed. If you have done this, then no action is required. If not, follow the link to reset it.",
                            ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + "/Account/ResetPassword",
                            "Reset now! It's an emergency!!1!",
                            ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + Url.Content("~/Home/EntertainmentHeading"));

                    model.Error = "ok";
                    return View(model);
                }
                model.Error = "error";
                return View(model);
            } else
            {
                //EMAIL CHANGE
                if (model.Password != null && model.Password.Length >= 6 && model.NewEmail != null && model.NewEmail.Length >= 2)
                {
                    DbUsersModel user = await dbUsers.DbUsers.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);

                    string hashed = "";
                    if (user != null)
                    {
                        // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                        hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                            password: model.Password,
                            salt: Convert.FromBase64String(user.Salt),
                            prf: KeyDerivationPrf.HMACSHA256,
                            iterationCount: 100000,
                            numBytesRequested: 256 / 8));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Не нашлось такого пользователя!");
                        model.ErrorEmail = "error";
                        return View(model);
                    }

                    if (user.Password != hashed)
                    {
                        ModelState.AddModelError("", "Некорректный пароль!");
                        model.ErrorEmail = "error";
                        return View(model);
                    }

                    if ((DateTime.Now - user.LastEmailConfirmDate).TotalHours <= 3)
                    {
                        model.ErrorEmail = "timeout";
                        return View(model);
                    }

                    string code = GetHashString(Guid.NewGuid().ToString() + user.Username);
                    var codeModel = new EmailCodeModel();
                    codeModel.EmailConfirmationCode = code;
                    codeModel.ConfirmationDate = DateTime.Now;
                    codeModel.Username = user.Username;
                    dbCodes.Add(codeModel);
                    await dbCodes.SaveChangesAsync();

                    await EmailService.SendEmailAsync(user.Language, user.Email, "Confirm your new Email", "Welcome. Again.",
                        user.Name + " " + user.Middlename,
                        user.Username,
                        "Hello! Confirm your new email address to make sure you entered it correctly again.",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + "/Account/ConfirmEmail?code=" + code,
                        "Confirm!",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + Url.Content("~/Home/EntertainmentHeading"));

                    user.LastEmailConfirmDate = DateTime.Now;
                    user.Email = model.NewEmail;
                    user.EmailConfirmed = false;
                    dbUsers.Update(user);
                    await dbUsers.SaveChangesAsync();

                    Log("Your Email has been changed!" + $" CurrentTime: {DateTime.Now}, User: {User.Identity.Name}, OldEmail: {model.Email}, NewEmail: {model.NewEmail}");

                    model.Email = model.NewEmail;
                    model.NewEmail = "";
                    model.ErrorEmail = "ok";
                }
                else model.ErrorEmail = "error";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetLog(DateTime Date)
        {
            var logPath = Path.Combine(webHostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
                TempData[User.Identity.Name + "_ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }
            var thisUserDirectory = Path.Combine(logPath, User.Identity.Name);
            if (!Directory.Exists(thisUserDirectory))
            {
                Directory.CreateDirectory(thisUserDirectory);
                TempData[User.Identity.Name + "_ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }
            if (!System.IO.File.Exists(Path.Combine(thisUserDirectory, Date.ToString("dd.MM.yyyy") + ".log")))
            {
                TempData[User.Identity.Name + "_ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }

            Log("Get Log was initiated! " + $"CurrentTime: {DateTime.Now}");
            return PhysicalFile(Path.Combine(thisUserDirectory, Date.ToString("dd.MM.yyyy") + ".log"), "text/plain", User.Identity.Name + "_" + Date.ToString("dd.MM.yyyy") + ".log");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeLanguage(string lang)
        {
            List<string> cultures = new List<string>() { "ru", "en" };
            if (!cultures.Contains(lang)) lang = "ru";

            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddHours(12);
            Response.Cookies.Append("lang", lang, option);

            var user = dbUsers.DbUsers.FirstOrDefault(m => m.Username == User.Identity.Name);
            if (user != null)
            {
                user.Language = lang;
                dbUsers.Update(user);
                await dbUsers.SaveChangesAsync();
            }

            Log("Language was changed! Current: " + lang + ", Username: " + User.Identity.Name + ", CurrentTime: " + DateTime.Now.ToString());

            return RedirectToAction("Settings", "Account");
        }

        [HttpGet]
        [Authorize]
        public IActionResult AdminPanel()
        {
            var user = dbUsers.DbUsers.FirstOrDefault(m => m.Username == User.Identity.Name);
            if (user != null && user.Role == "Ranged-Creep")
            {
                TempData[User.Identity.Name + "_Role"] = user.Role;
                IEnumerable<DbUsersModel> model = dbUsers.DbUsers.Where(m => true);
                return View(model);
            }
            else return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminPanel(string dataSend, string dataRegistration)
        {
            if (dataRegistration == null || (dataRegistration != "open" && dataRegistration != "closed")) return RedirectToAction("AdminPanel", "Account");
            bool registration = (dataRegistration == "open") ? true : false;
            if (registration != Startup.RegistrationOpen)
            {
                Startup.RegistrationOpen = registration;
                var path = Path.Combine(webHostEnvironment.ContentRootPath, "RegistrationOpen.txt");
                if (!System.IO.File.Exists(path)) System.IO.File.Create(path);
                System.IO.File.WriteAllText(path, registration ? "1" : "0");

                Log((registration ? "Registration Open" : "Registration Closed") + $", CurrentTime: {DateTime.Now}" + $"Username: {User.Identity.Name}");
            }

            if (dataSend == null || dataSend.Length <= 5) return RedirectToAction("AdminPanel", "Account");
            string lang = "ru";
            if (Request.Cookies.ContainsKey("lang")) lang = Request.Cookies["lang"];

            var user = dbUsers.DbUsers.FirstOrDefault(m => m.Username == User.Identity.Name);
            if (user != null && user.Role == "Ranged-Creep")
            {
                var splited = dataSend.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var a in splited)
                {
                    var miniSplited = a.Split('_', StringSplitOptions.RemoveEmptyEntries);
                    var thisuser = dbUsers.DbUsers.FirstOrDefault(m => m.Username == Encoding.UTF8.GetString(Convert.FromBase64String(miniSplited[0])));
                    if (thisuser.Role == "Ranged-Creep" || (thisuser.Approved == (miniSplited[1] == "1" ? "true" : "false") && thisuser.Role == (miniSplited[2] == "1" ? "Ranged-Creep" : "Creep"))) continue;
                    thisuser.Approved = miniSplited[1] == "1" ? "true" : "false";
                    thisuser.Role = miniSplited[2] == "1" ? "Ranged-Creep" : "Creep";
                    TempData.Remove(thisuser.Username + "_Role");
                    TempData[thisuser.Username + "_Role"] = thisuser.Role;

                    if (thisuser.Approved == "true")
                    {
                        //You account has been activated
                        await EmailService.SendEmailAsync(thisuser.Language, thisuser.Email, "Your account has been activated", "Welcome",
                        thisuser.Name + " " + thisuser.Middlename,
                        thisuser.Username,
                        "Hello! Your account has just been activated, which means that you can safely start using the system to its fullest! Come in, look around, join - there is a button below!",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()),
                        "First login!",
                        ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + Url.Content("~/Home/EntertainmentHeading"));
                    }

                    Log("Approved/Role changed: " + $"CurrentTime: {DateTime.Now}" + $"Username: {User.Identity.Name}, " + $"Object's nickname: {thisuser.Username}, approved: {thisuser.Approved}, role: {thisuser.Role}");
                    dbUsers.Update(thisuser);
                }

                await dbUsers.SaveChangesAsync();

                IEnumerable<DbUsersModel> model = dbUsers.DbUsers.Where(m => true);
                return View(model);
            }
            else return RedirectToAction("AdminPanel", "Account");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            if (code == null || code.Length <= 4) return RedirectToAction("Index", "Home");

            var codeModel = dbCodes.DbCodes.FirstOrDefault(m => m.EmailConfirmationCode == code && (m.ConfirmationDate - DateTime.Now).TotalHours > 3);
            if (codeModel == null) return RedirectToAction("Index", "Home");

            var user = dbUsers.DbUsers.FirstOrDefault(m => m.Username == codeModel.Username && !m.EmailConfirmed);
            if (user != null)
            {
                dbCodes.Remove(codeModel);
                await dbCodes.SaveChangesAsync();

                user.EmailConfirmed = true;
                dbUsers.Update(user);
                await dbUsers.SaveChangesAsync();

                Log("Email confirmed! " + $"CurrentTime: {DateTime.Now}" + $"Username: {user.Username}, " + $"Code: {codeModel.EmailConfirmationCode}", user.Username);

                return View(new Tuple<string, string>(user.Username, user.Email));
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code)
        {
            //Adding "default" model
            ResetPasswordModel model = new ResetPasswordModel();
            model.Case = 2;
            if (code == null) return View(model);

            var codeModel = dbCodes.DbCodes.FirstOrDefault(m => m.EmailChangePasswordCode == code);
            if (codeModel != null) model.Case = 1;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            model.Case = 2;

            if ((model.Username == null && model.Email == null) || (model.Username != null && model.Username.Length <= 4) || (model.Email != null && model.Email.Length <= 2))
            {
                model.Error = "error";
                return View(model);
            }

            DbUsersModel user = dbUsers.DbUsers.FirstOrDefault(m => (m.Username == model.Username || m.Email == model.Email) && m.Approved == "true" && m.EmailConfirmed);
            if (user == null)
            {
                user = dbUsers.DbUsers.FirstOrDefault(m => m.Email == model.Username && m.Approved == "true" && m.EmailConfirmed);
                if (user == null)
                {
                    model.Error = "error";
                    return View(model);
                }
            }

            bool NEW = false;
            var codeModel = dbCodes.DbCodes.FirstOrDefault(m => m.Username == user.Username);
            if (codeModel != null && (DateTime.Now - codeModel.ChangePasswordDate).TotalHours <= 3)
            {
                model.Error = "timeout";
                return View(model);
            }
            if (codeModel == null)
            {
                string code = GetHashString(Guid.NewGuid().ToString() + user.Username);
                codeModel = new EmailCodeModel();
                codeModel.EmailChangePasswordCode = code;
                codeModel.ChangePasswordDate = DateTime.Now;
                codeModel.Username = user.Username;
                NEW = true;

                dbCodes.Add(codeModel);
                await dbCodes.SaveChangesAsync();
            }

            Log("Password recovery requested! " + $"CurrentTime: {DateTime.Now}" + $", Username: {user.Username}, " + $"Code: {codeModel.EmailChangePasswordCode}, " + $"NEW: {NEW}", user.Username);

            if (NEW || (codeModel != null && (DateTime.Now - codeModel.ChangePasswordDate).TotalHours > 3))
            {
                await EmailService.SendEmailAsync(user.Language, user.Email, "Confirm your Email", "Welcome",
                user.Name + " " + user.Middlename,
                user.Username,
                "Hello! Confirm your email address to make sure you entered it correctly again.",
                ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + "/Account/ResetPassword?code=" + codeModel.EmailChangePasswordCode,
                "Confirm!",
                ((Request.Host.Host.StartsWith("http://") || Request.Host.Host.StartsWith("https://")) ? "" : "https://") + Request.Host.Host + (Request.Host.Port == 80 ? "" : ":" + Request.Host.Port.ToString()) + Url.Content("~/Home/EntertainmentHeading"));

                Log("Password recovery email has been sent! " + $"CurrentTime: {DateTime.Now}" + $", Username: {user.Username}, " + $"Code: " + codeModel.EmailChangePasswordCode, user.Username);
            }

            model.Error = "ok";
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPasswordExact(ResetPasswordExactModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.NewPassword != model.NewPasswordConfirm)
                {
                    model.Error = "error";
                    return View(model);
                }

                var codeModel = dbCodes.DbCodes.FirstOrDefault(m => m.EmailChangePasswordCode == model.Code);
                var user = dbUsers.DbUsers.FirstOrDefault(m => m.Username == codeModel.Username);
                if (codeModel == null)
                {
                    model.Error = "error";
                    return View(model);
                }
                dbCodes.Remove(codeModel);
                await dbCodes.SaveChangesAsync();

                //Generate a 128-bit salt using a cryptographically strong random sequence of nonzero values
                byte[] salt = new byte[128 / 8];
                using (var rngCsp = new RNGCryptoServiceProvider())
                {
                    rngCsp.GetNonZeroBytes(salt);
                }
                //Derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: model.NewPassword,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100000,
                    numBytesRequested: 256 / 8));

                //Updating user back to DB
                user.Password = hashed;
                user.Salt = Convert.ToBase64String(salt);
                dbUsers.Update(user);
                await dbUsers.SaveChangesAsync();

                Log("Your password has been changed by \"Reset password\"! " + $"CurrentTime: {DateTime.Now}" + $"Username: {user.Username}", user.Username);

                model.Error = "ok";
            }
            return View(model);
        }

        public string GetHashString(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = string.Empty;

            foreach (byte b in byteHash) hash += string.Format("{0:x2}", b);

            return hash;
        }
    }
}
