using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProyectoEcommerce.Migrations;
using ProyectoEcommerce.Models;
using ProyectoEcommerce.Models.Data;
using ProyectoEcommerce.Models.Entidades;
using ProyectoEcommerce.Services;
using Vereyon.Web;

namespace ProyectoEcommerce
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<EcommerceContext>(o =>
            {
                o.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSQL"));
            });
            builder.Services.AddTransient<SeedDb>();
            builder.Services.AddScoped<IServicioUsuario, ServicioUsuario>();
            builder.Services.AddScoped<IServicioLista, ServicioLista>();
            builder.Services.AddScoped<IServicioImagen, ServicioImagen>();
            builder.Services.AddScoped<IServicioVenta, ServicioVenta>();
            builder.Services.AddFlashMessage();

            builder.Services.AddIdentity<Usuario, IdentityRole>(cfg =>
            {
                cfg.User.RequireUniqueEmail = true;
                cfg.Password.RequireDigit = false;
                cfg.Password.RequiredUniqueChars = 0;
                cfg.Password.RequireLowercase = false;
                cfg.Password.RequireNonAlphanumeric = false;
                cfg.Password.RequireUppercase = false;
            }).AddEntityFrameworkStores<EcommerceContext>();
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login/NoAutorizado";
                options.AccessDeniedPath = "/Login/NoAutorizado";
            });

            var app = builder.Build();

            SeedData(app);
            void SeedData(WebApplication app)
            {
                IServiceScopeFactory scopedFactory = app.Services.GetService<IServiceScopeFactory>();
                using (IServiceScope scope = scopedFactory.CreateScope())
                {
                    SeedDb service = scope.ServiceProvider.GetService<SeedDb>();
                    service.SeedAsync().Wait();
                }
            }


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
            app.UseStatusCodePagesWithReExecute("/error/{0}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
