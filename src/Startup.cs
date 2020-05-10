using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            services.AddAutoMapper(typeof(Startup));
            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 0);
                opt.ReportApiVersions = true;
                //opt.ApiVersionReader = new QueryStringApiVersionReader("ver");
                //opt.ApiVersionReader = new HeaderApiVersionReader("X-Version");
                opt.ApiVersionReader = ApiVersionReader.Combine(
                    new QueryStringApiVersionReader("ver", "version"),
                    new HeaderApiVersionReader("X-Version"));

                // opt.ApiVersionReader = new UrlSegmentApiVersionReader();

                //opt.Conventions.Controller<TalksController>()
                //.HasDeprecatedApiVersion(1, 0)
                //.HasApiVersion(new ApiVersion(1, 0));
                
            });
            services.AddMvc(opt => opt.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
