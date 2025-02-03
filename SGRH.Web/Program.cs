using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGRH.Web.Controllers;
using SGRH.Web.Models;
using SGRH.Web.Models.Data;
using SGRH.Web.Models.Entities;
using SGRH.Web.Services;
using System;
using System.Configuration;

namespace SGRH.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<SgrhContext>(o =>
            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("SGRHDB"));
            });

            builder.Services.AddScoped<IServiceUser, ServiceUser>();
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<IAbsenceService, AbsenceService>();
            builder.Services.AddScoped<IDossierService, DossierService>();
            builder.Services.AddScoped<IPersonalActionService, PersonalActionService>();
            builder.Services.AddScoped<IVacationService, VacationService>();
            builder.Services.AddScoped<IWarningService, WarningService>();
            builder.Services.AddScoped<IOvertimeService, OverTimeService>();
            builder.Services.AddScoped<IPayrollService, PayrollService>();
            builder.Services.AddScoped<IPayrollPeriodService, PayrollPeriodService>();
            builder.Services.AddScoped<ILayoffsService, LayoffsService>();
            builder.Services.AddScoped<ISettlementService, SettlementService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IPositionService, PositionService>();

             // Agrega al siguiente código lo necesario para que en la base de datos se almacene de manera correcta el lockoutend
            builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<SgrhContext>()
            .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "AspNetCore.Identity.Application";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(5); // La cookie expira al finalizar la sesi�n del navegador
                options.SlidingExpiration = true;
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                //options.Cookie.IsEssential = true; // La cookie es esencial y se elimina al cerrar el navegador
                //options.Cookie.SameSite = SameSiteMode.Lax; // Configuraci�n adicional para la cookie
                //options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Configuraci�n adicional para la cookie
            });

            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddTransient<EmailService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
