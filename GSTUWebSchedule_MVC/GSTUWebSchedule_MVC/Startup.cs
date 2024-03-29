using GSTUWebSchedule_MVC.Controllers;
using GSTUWebSchedule_MVC.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;

namespace GSTUWebSchedule_MVC
{
    public class Startup
    {
        public static bool RegistrationOpen = true;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            //Adding MobileContext like a service in app
            services.AddDbContext<DbTableContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddDbContext<DbUsersContext>(options => options.UseSqlServer(Configuration.GetConnectionString("UsersDbConnection")));
            services.AddDbContext<LastVisitsContext>(options => options.UseSqlServer(Configuration.GetConnectionString("LastVisitsConnection")));
            services.AddDbContext<DbEmailCodesContext>(options => options.UseSqlServer(Configuration.GetConnectionString("EmailCodesConnection")));

            // установка конфигурации подключения
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => //CookieAuthenticationOptions
                {
                    options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
                    options.LogoutPath = new Microsoft.AspNetCore.Http.PathString("/Account/Logout");
                    options.ExpireTimeSpan = TimeSpan.FromHours(12);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var path = Path.Combine(env.ContentRootPath, "RegistrationOpen.txt");
            if (!File.Exists(path))
            {
                File.AppendAllText(path, "1");
            }
            var content = File.ReadAllText(path);
            if (content == "1") RegistrationOpen = true; else RegistrationOpen = false;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
