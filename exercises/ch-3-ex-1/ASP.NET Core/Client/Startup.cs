using System;

using Client.Authentication.OAuth;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Client
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
            });

            services.AddControllersWithViews();

            services.AddAuthentication("OAuth")
                    .AddRemoteScheme<OAuthAuthenticationOptions, OAuthAuthenticationHandler>("OAuth", "OAuth", options =>
                    {
                        options.AuthorizationEndpoint = new Uri("http://localhost:9001/authorize");
                        options.CallbackPath = new PathString("/callback");
                        options.ClientId = "oauth-client-1";
                        options.ClientSecret = "oauth-client-secret-1";
                        options.ResponseType = "code";
                        options.TokenEndpoint = new Uri("http://localhost:9001/token");
                    });

            services.AddHttpClient();
        }
    }
}