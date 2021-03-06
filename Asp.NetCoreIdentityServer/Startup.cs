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
            services.AddTransient<IAuthorizationHandler, ExpireDateExchangeHandler>(); //her karşılaştığında bir tane üreticek
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
                opts.User.AllowedUserNameCharacters = "abcçdefgğhıijklmnoöpqrsştuüvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._";

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
            cookieBuilder.HttpOnly = false; //saldırılarda client-side tarafından cookie'lere erişemez.
                                            // sadece http isteği üzerinden cookie bilgisi alınabilir.


            cookieBuilder.SameSite = SameSiteMode.Lax; //Siteler arası cookie taşınır
                                                       //Strict ise bankalar vb. kurumlar tarafından kullanılır cookieye farklı siteden erişilemez taşınamaz.

            cookieBuilder.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            //SameAsRequest : Cookie Http üzerinden geldiyse Http üzerinden cookieyi gönderir
            //                Cookie Https üzerinden geldiyse Https üzerinden cookieyi gönderir
            //Always : Cookie sadece Https üzerinden geldiyse gönderir.
            //None : Protokoller önemsizdir.

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = new PathString("/Home/Login"); // Giriş yapmadıysa kullanıcı login sayfasına otomatik yönlendiriyoruz.
                options.LogoutPath = new PathString("/Member/Logout");
                options.Cookie = cookieBuilder;
                options.ExpireTimeSpan = TimeSpan.FromDays(60);// cookie tutma değeri 60 gün
                options.SlidingExpiration = true; //Kullanıcı belirtilen 60 günün 30 gününden sonra tekrar girdiyse cookileri 60 gün daha otomatik olarak uzatılır.
                options.AccessDeniedPath = new PathString("/Member/AccessDenied"); // Kullanıcı yetkisinin olmadığı erişemiceği alanlarda bilgi vermek için yönlendirme yapıyoruz. 
            });

            services.AddScoped<IClaimsTransformation, ClaimProvider.ClaimProvider>();

            services.AddMvc(option => option.EnableEndpointRouting = false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage(); //Sayfada hata aldığın zaman hatayla ilgili açıklayıcı bilgi sunuyor
            app.UseStatusCodePages(); //Boş bir sayfa dönmek yerine hatanın nerde olduğunu gösteren yazı gösteriyor

            app.UseStaticFiles(); // js , css dosyalarını yükleyip çalıştırabilmek için staticfiles ekliyoruz.
            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
            
        }
    }
}
