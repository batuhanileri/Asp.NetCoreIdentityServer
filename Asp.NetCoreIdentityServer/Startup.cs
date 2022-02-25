using Asp.NetCoreIdentityServer.CustomValidation;
using Asp.NetCoreIdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Asp.NetCoreIdentityServer
{
    public class Startup
    {
        public IConfiguration configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionString"]));

            

            services.AddIdentity<AppUser, AppRole>(opts=> 
            {
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abcçdefghýijklmnoöpqrsþtuüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

                opts.Password.RequireDigit = false;
                opts.Password.RequireUppercase = false;
                opts.Password.RequireLowercase = false;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Password.RequiredLength = 4;
            }).AddPasswordValidator<CustomPasswordValidator>()
              .AddUserValidator<CustomUserValidator>() 
              .AddErrorDescriber<CustomIdentityErrorDescriber>()
              .AddEntityFrameworkStores<AppIdentityDbContext>()
              .AddDefaultTokenProviders();


            CookieBuilder cookieBuilder = new CookieBuilder();

            cookieBuilder.Name = "MyBlog";
            cookieBuilder.HttpOnly = false; //saldýrýlarda client-side tarafýndan cookie'lere eriþemez.
                                            // sadece http isteði üzerinden cookie bilgisi alýnabilir.


            cookieBuilder.SameSite = SameSiteMode.Lax; //Siteler arasý cookie taþýnýr
                                                       //Strict ise bankalar vb. kurumlar tarafýndan kullanýlýr cookieye farklý siteden eriþilemez taþýnamaz.

            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            //SameAsRequest : Cookie Http üzerinden geldiyse Http üzerinden cookieyi gönderir
            //                Cookie Https üzerinden geldiyse Https üzerinden cookieyi gönderir
            //Always : Cookie sadece Https üzerinden geldiyse gönderir.
            //None : Protokoller önemsizdir.

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Home/Login"); // Giriþ yapmadýysa kullanýcý login sayfasýna otomatik yönlendiriyoruz.
                options.Cookie = cookieBuilder;
                options.ExpireTimeSpan = TimeSpan.FromDays(60);// cookie tutma deðeri 60 gün
                options.SlidingExpiration = true; //Kullanýcý belirtilen 60 günün 30 gününden sonra tekrar girdiyse cookileri 60 gün daha otomatik olarak uzatýlýr.
            });

            services.AddMvc(option => option.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage(); //Sayfada hata aldýðýn zaman hatayla ilgili açýklayýcý bilgi sunuyor
            app.UseStatusCodePages(); //Boþ bir sayfa dönmek yerine hatanýn nerde olduðunu gösteren yazý gösteriyor

            app.UseStaticFiles(); // js , css dosyalarýný yükleyip çalýþtýrabilmek için staticfiles ekliyoruz.
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            
        }
    }
}
