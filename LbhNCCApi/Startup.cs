using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Reflection;
using LbhNCCApi.Interfaces;
using LbhNCCApi.Actions;

namespace LbhNCCApi
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();
            services.AddTransient<ICRMClientActions, AccessTokenService>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info
                {
                    Version = "v1",
                    Title = $"LBH NCC API - {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}",
                    Description = "This is the LBH NCC Api."
                });
                c.DescribeAllEnumsAsStrings();
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddCors(option => {
                option.AddPolicy("AllowAny", policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            });
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCors("AllowAny");
            app.UseMvc();
            app.UseDeveloperExceptionPage();
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    string basePath = Environment.GetEnvironmentVariable("NCC_ASPNETCORE_APPL_PATH");
                    if (basePath == null) basePath = "/";

                    c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "lbhnccapi");
                    //c.SpecUrl = $"{basePath}swagger/v1/swagger.json";
                    //c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Test")
                {
                    app.UseSwagger(
                        c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "sandboxapi.hackney.gov.uk/lbhnccapi/")
                    );
                }
                else
                {
                    app.UseSwagger(
                        c => c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = "api.hackney.gov.uk/lbhnccapi/")
                    );
                }
                app.UseSwaggerUI(c =>
                {
                    string basePath = Environment.GetEnvironmentVariable("NCC_ASPNETCORE_APPL_PATH");
                    if (basePath == null) basePath = "/lbhnccapi/";
                    c.SwaggerEndpoint($"{basePath}swagger/v1/swagger.json", "LbhNCCApi");
                    //c.RoutePrefix = string.Empty;
                });
            }
        }
    }
}