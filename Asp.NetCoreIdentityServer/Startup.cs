using Asp.NetCoreIdentityServer.CustomValidation;
using Asp.NetCoreIdentityServer.Models;
using Asp.NetCoreIdentityServer.Requirement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>(); //her kar��la�t���nda bir tane �reticek
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(configuration["ConnectionStrings:DefaultConnectionString"]));

            services.AddAuthorization(opts =>
            {
                opts.AddPolicy("CityPolicy", policy =>
                 {
                     policy.RequireClaim("City", "Bursa");
                 });

                opts.AddPolicy("BirthDayPolicy", policy =>
                {
                    policy.RequireClaim("BirthDay");
                });
                opts.AddPolicy("ExchangePolicy", policy =>
                {
                    policy.AddRequirements(new ExpireDateExchangeRequirement());
                });
            });

            services.AddAuthentication().AddGoogle(opts =>
            {
                opts.ClientId = configuration["Authentication:Google:ClientID"];
                opts.ClientSecret = configuration["Authentication:Google:ClientSecret"];
            });

            services.AddIdentity<AppUser, AppRole>(opts=> 
            {
                opts.User.RequireUniqueEmail = true;
                opts.User.AllowedUserNameCharacters = "abc�defg�h�ijklmno�pqrs�tu�vwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

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
            cookieBuilder.HttpOnly = false; //sald�r�larda client-side taraf�ndan cookie'lere eri�emez.
                                            // sadece http iste�i �zerinden cookie bilgisi al�nabilir.


            cookieBuilder.SameSite = SameSiteMode.Lax; //Siteler aras� cookie ta��n�r
                                                       //Strict ise bankalar vb. kurumlar taraf�ndan kullan�l�r cookieye farkl� siteden eri�ilemez ta��namaz.

            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            //SameAsRequest : Cookie Http �zerinden geldiyse Http �zerinden cookieyi g�nderir
            //                Cookie Https �zerinden geldiyse Https �zerinden cookieyi g�nderir
            //Always : Cookie sadece Https �zerinden geldiyse g�nderir.
            //None : Protokoller �nemsizdir.

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Home/Login"); // Giri� yapmad�ysa kullan�c� login sayfas�na otomatik y�nlendiriyoruz.
                options.LogoutPath = new PathString("/Member/Logout");
                options.Cookie = cookieBuilder;
                options.ExpireTimeSpan = TimeSpan.FromDays(60);// cookie tutma de�eri 60 g�n
                options.SlidingExpiration = true; //Kullan�c� belirtilen 60 g�n�n 30 g�n�nden sonra tekrar girdiyse cookileri 60 g�n daha otomatik olarak uzat�l�r.
                options.AccessDeniedPath = new PathString("/Member/AccessDenied"); // Kullan�c� yetkisinin olmad��� eri�emice�i alanlarda bilgi vermek i�in y�nlendirme yap�yoruz. 
            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();

            services.AddMvc(option => option.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage(); //Sayfada hata ald���n zaman hatayla ilgili a��klay�c� bilgi sunuyor
            app.UseStatusCodePages(); //Bo� bir sayfa d�nmek yerine hatan�n nerde oldu�unu g�steren yaz� g�steriyor

            app.UseStaticFiles(); // js , css dosyalar�n� y�kleyip �al��t�rabilmek i�in staticfiles ekliyoruz.
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            
        }
    }
}
