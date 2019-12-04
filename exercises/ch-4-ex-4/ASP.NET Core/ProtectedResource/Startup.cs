using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

using ProtectedResource.Database;
using ProtectedResource.OAuth;

namespace ProtectedResource
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(x => x.AllowAnyOrigin());
            app.UseAuthentication();
            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var mvcCoreBuilder = services.AddMvcCore();
            mvcCoreBuilder.AddAuthorization();
            mvcCoreBuilder.AddRazorViewEngine();
            mvcCoreBuilder.AddCors();
            mvcCoreBuilder.AddJsonFormatters();

            services.AddScoped<INoSql>(x => new NoSql(@"..\..\database.nosql"));

            services.AddAuthentication("OAuthAccessToken").AddScheme<AuthenticationSchemeOptions, OAuthAccessTokenHandler>("OAuthAccessToken", null);
        }
    }
}