using System;

using Client.Authentication.OAuth;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            services.AddControllersWithViews();

            services.AddAuthentication("OAuth")
                    .AddScheme<OAuthAuthenticationOptions, OAuthAuthenticationHandler>("OAuth", options =>
                    {
                        options.AuthorizationEndpoint = new Uri("http://localhost:9001/authorize");
                        options.ClientId = "oauth-client-1";
                        options.ClientSecret = "oauth-client-secret-1";
                        options.RedirectEndpoint = new Uri("http://localhost:9000/callback");
                        options.ResponseType = "code";
                        options.TokenEndpoint = new Uri("http://localhost:9001/token");
                    });
        }
    }
}