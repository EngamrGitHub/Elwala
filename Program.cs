using Microsoft.EntityFrameworkCore;

namespace Elwala
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                });
            builder.Services.AddHttpClient<Elwala.Services.IAffiliateService, Elwala.Services.AffiliateService>(client => 
            {
                client.BaseAddress = new Uri("https://api.ellwaa.com/"); 
                var token = builder.Configuration["ApiSettings:Token"];
                if (!string.IsNullOrEmpty(token) && token != "YOUR_API_TOKEN_HERE")
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }
            });

            builder.Services.AddDbContext<Elwala.Data.ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
                pattern: "{controller=Account}/{action=Login}/{id?}");

            // Auto-apply any pending migrations on startup (creates tables automatically)
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<Elwala.Data.ApplicationDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}
