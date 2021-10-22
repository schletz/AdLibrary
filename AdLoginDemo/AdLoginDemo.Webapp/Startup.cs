using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AdLoginDemo.Application.Infrastructure;
using AdLoginDemo.Webapp.Services;

namespace AdLoginDemo.Webapp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            HostEnvironment = hostEnvironment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostEnvironment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(provider => new LoginService(
                provider.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Configuration["Searchuser"],
                Configuration["Searchpass"],
                HostEnvironment.IsDevelopment() && !string.IsNullOrEmpty(Configuration["Searchuser"])));

            services.AddRazorPages();
            services.AddAuthentication(
                Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = "/Login";
                    o.AccessDeniedPath = "/NotAuthorized";
                });
            services.AddAuthorization(o =>
            {
                o.AddPolicy("RequirePupilRole", p => p.RequireRole(AdUserRole.Administration.ToString(), AdUserRole.Management.ToString(), AdUserRole.Teacher.ToString(), AdUserRole.Pupil.ToString()));
                o.AddPolicy("RequireTeacherRole", p => p.RequireRole(AdUserRole.Administration.ToString(), AdUserRole.Management.ToString(), AdUserRole.Teacher.ToString()));
                o.AddPolicy("RequireAdministrationRole", p => p.RequireRole(AdUserRole.Administration.ToString(), AdUserRole.Management.ToString()));
                o.AddPolicy("RequireManagementRole", p => p.RequireRole(AdUserRole.Management.ToString()));
            });
            services.AddHttpContextAccessor();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Setzt Security Header. Ohne await next() würde ein "short circuit" entstehen und die
            // Pipeline abbrechen. Achtung: Das Schreiben in die Response ist im Allgemeinen ein
            // Antipattern, es sollte nur sehr gezielt verwendet werden.
            // Siehe: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Referrer-Policy", "no-referrer");
                context.Response.Headers.Add("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
                // https://wiki.selfhtml.org/wiki/Sicherheit/Content_Security_Policy
                //context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src *");
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}