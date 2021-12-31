using GSTUWebSchedule_MVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace GSTUWebSchedule_MVC.Controllers
{
    public class AccountController : Controller
    {
        private DbUsersContext dbUsers;
        public AccountController(DbUsersContext context)
        {
            dbUsers = context;
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

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Некорректные логин или пароль!");
            }
            return View(model);
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
                /*if (model.Password != model.ConfirmPassword)
                {
                    return View(model);
                }*/

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

                    // добавляем пользователя в бд
                    dbUsers.DbUsers.Add(new DbUsersModel
                    {
                        Username = model.Username,
                        Password = hashed,
                        Salt = Convert.ToBase64String(salt),
                        Email = model.Email,
                        Name = model.Name,
                        Surname = model.Surname,
                        Middlename = model.Middlename,
                        Approved = "false"
                    });
                    await dbUsers.SaveChangesAsync();

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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/Account/Login/?logout=1");
        }
    }
}
