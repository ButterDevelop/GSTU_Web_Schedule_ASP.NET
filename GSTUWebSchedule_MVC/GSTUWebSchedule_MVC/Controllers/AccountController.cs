using GSTUWebSchedule_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UAParser;

namespace GSTUWebSchedule_MVC.Controllers
{
    public class AccountController : Controller
    {
        private DbUsersContext dbUsers;
        private LastVisitsContext lastVisits;
        private IWebHostEnvironment webHostEnvironment;

        public AccountController(DbUsersContext context, LastVisitsContext context2, IWebHostEnvironment _webHostEnvironment)
        {
            dbUsers = context;
            lastVisits = context2;
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
                    await Authenticate(model.Username); // аутентификация

                    var visits = lastVisits.DbTable.Where(m => m.Username == model.Username);
                    if (visits.Count(m => m.Username == model.Username) >= 10)
                    {
                        var min = visits.Min(m => m.Date);
                        for (int i = 0; i < visits.Count(m => m.Username == model.Username) - 9; i++) lastVisits.DbTable.Remove(visits.Where(m => m.Date == min).ToArray()[i]);
                    }

                    var userAgent = HttpContext.Request.Headers["User-Agent"];
                    var uaParser = Parser.GetDefault();
                    ClientInfo c = uaParser.Parse(userAgent);
                    var currentvisit = new LastVisitsModel() { Username = model.Username, Date = DateTime.Now, FullUserAgent = c.String, OS = c.OS.ToString(), Browser = c.UA.ToString(), IP = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString(), isMobile = fBrowserIsMobile(c.String) };
                    lastVisits.Add(currentvisit);

                    await lastVisits.SaveChangesAsync();

                    Log("Login was initiated! " + $"Username: {currentvisit.Username}, Date: {currentvisit.Date}, FullUserAgent: {currentvisit.FullUserAgent}, OS: {currentvisit.OS}, Browser: {currentvisit.Browser}, IP: {currentvisit.IP}, isMobile: {currentvisit.isMobile}", currentvisit.Username);

                    CheckLogs(currentvisit.Username);

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
                        Name = model.Name,
                        Surname = model.Surname,
                        Middlename = model.Middlename,
                        RegisterTime = DateTime.Now,
                        Approved = "false",
                        Role = "Creep"
                    };
                    dbUsers.DbUsers.Add(registeruser);
                    await dbUsers.SaveChangesAsync();
                    
                    Log("Your account has been registered! " + $"Username: {registeruser.Username}, RegisterTime: {registeruser.RegisterTime}", registeruser.Username);

                    var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                    var result = await UserManager.CreateAsync(user, model.Password);
                    if (result.Succeeded)
                    {
                        // если создание прошло успешно, то добавляем роль пользователя
                        await UserManager.AddToRoleAsync(user.Id, "user");
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        return RedirectToAction("Index", "Home");
                    }
                    AddErrors(result);

                    return RedirectToAction("Index", "Home");
                }
                else
                    ModelState.AddModelError("", "Введённые данные уже заняты");
            }
            return View(model);
        }

        private async Task Authenticate(string userName)
        {
            // создаем один claim
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            // создаем объект ClaimsIdentity
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            // установка аутентификационных куки
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
            SettingsModel model = new SettingsModel();
            if (TempData["ErrorLogs"] != null) { model.ErrorLogs = TempData["ErrorLogs"].ToString(); TempData["ErrorLogs"] = null; }
            model.LastVisitsTable = lastVisits.DbTable.Where(m => m.Username == User.Identity.Name);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(SettingsModel model)
        {
            model.LastVisitsTable = lastVisits.DbTable.Where(m => m.Username == User.Identity.Name);
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

                model.Error = "ok";
                return View(model);
            }
            model.Error = "error";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetLog(DateTime Date)
        {
            var logPath = Path.Combine(webHostEnvironment.ContentRootPath, "Logs");
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
                TempData["ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }
            var thisUserDirectory = Path.Combine(logPath, User.Identity.Name);
            if (!Directory.Exists(thisUserDirectory))
            {
                Directory.CreateDirectory(thisUserDirectory);
                TempData["ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }
            if (!System.IO.File.Exists(Path.Combine(thisUserDirectory, Date.ToString("dd.MM.yyyy") + ".log")))
            {
                TempData["ErrorLogs"] = "error";
                return RedirectToAction("Settings", "Account");
            }

            Log("Get Log was initiated! " + $"CurrentTime: {DateTime.Now}");
            return PhysicalFile(Path.Combine(thisUserDirectory, Date.ToString("dd.MM.yyyy") + ".log"), "text/plain", User.Identity.Name + "_" + Date.ToString("dd.MM.yyyy") + ".log");
        }
    }
}
