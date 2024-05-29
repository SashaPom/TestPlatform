using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TestPlatform.Domain;
using TestPlatform.Middleware;
using TestPlatform.Services.Hash;
using TestPlatform.Services.Kdf;
using TestPlatform.Services.RandomService;
using TestPlatform.Services.Test;
using TestPlatform.Services.Validation;

namespace TestPlatform
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IValidationService, ValidationService>();
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<IRandomService, RandomService>();
            services.AddSingleton<IHashService, HashService>();
            services.AddSingleton<IKdfService, KdfService>();

            services.AddControllersWithViews();
            services.AddRazorPages();            

            services.AddDbContext<TestPlatrormDbContext>(options
                 => options.UseSqlServer(Configuration.GetConnectionString("MyConnectionString")));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(180);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            //app.UseStatusCodePagesWithRedirects("/Home/Error?statuscode={0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseSession();
            app.UseSessionAuth();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Auth}/{id?}",
                    defaults: new { controller = "Account", action = "Auth" }
                );
                endpoints.MapRazorPages();
            });
        }
    }
}
