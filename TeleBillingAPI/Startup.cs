﻿
using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TeleBillingAPI.Helpers;
using TeleBillingRepository.Repository.Account;
using TeleBillingRepository.Repository.BillDelegate;
using TeleBillingRepository.Repository.BillMemo;
using TeleBillingRepository.Repository.BillProcess;
using TeleBillingRepository.Repository.BillUpload;
using TeleBillingRepository.Repository.Configuration;
using TeleBillingRepository.Repository.Dashboard;
using TeleBillingRepository.Repository.Employee;
using TeleBillingRepository.Repository.Master.ExcelMapping;
using TeleBillingRepository.Repository.Master.HandsetManagement;
using TeleBillingRepository.Repository.Master.InternetDevice;
using TeleBillingRepository.Repository.Master.RoleManagement;
using TeleBillingRepository.Repository.Operator;
using TeleBillingRepository.Repository.Package;
using TeleBillingRepository.Repository.Provider;
using TeleBillingRepository.Repository.Report;
using TeleBillingRepository.Repository.StaticData;
using TeleBillingRepository.Repository.Telephone;
using TeleBillingRepository.Repository.Template;
using TeleBillingRepository.Service;
using TeleBillingRepository.Service.Constants;
using TeleBillingRepository.Service.LogMangement;
using TeleBillingUtility.Models;

namespace TeleBillingAPI
{
    public class Startup
    {
        public IConfiguration Configuration { get; }


        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CORS", corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin()

                    .WithOrigins(Configuration["OriginUrlLive"], Configuration["OriginUrlLocal"])
                    // Apply CORS policy for any type of origin  
                    .AllowAnyMethod()
                    // Apply CORS policy for any type of http methods  
                    .AllowAnyHeader()
                    // Apply CORS policy for any headers  
                    .AllowCredentials());
                // Apply CORS policy for all users  
            });

            //Add framework services.
            services.AddDbContext<telebilling_v01Context>(options =>
            options.UseMySql(Configuration.GetConnectionString("DefaultConnection")));


            //Register application services
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IStringConstant, StringConstant>();
            services.AddScoped<IProviderRepository, ProviderRepository>();
            services.AddScoped<ILogManagement, LogManagement>();
            services.AddScoped<IStaticDataRepository, StaticDataRepository>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IPackageRepository, PackageRepository>();
            services.AddScoped<IExcelMappingRepository, ExcelMappingRepository>();
            services.AddScoped<IHandsetRepository, HandsetRepository>();
            services.AddScoped<ITelephoneRepository, TelephoneRepository>();
            services.AddScoped<IBillUploadRepository, BillUploadRepository>();
            services.AddScoped<ITemplateRepository, TemplateRepository>();
            services.AddScoped<IOperatorRepository, OperatorRepository>();
            services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
            services.AddScoped<IinternetDeviceRepositoy, InternetDeviceRepositoy>();
            services.AddScoped<IBillProcessRepository, BillProcessRepository>();
            services.AddScoped<IBillDelegateRepository, BillDelegateRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<IBillMemoRepository, BillMemoRepository>();
            services.AddScoped<IReportRepository, ReportRepository>();
            services.AddScoped<IDashboardRepository, DashboardRepository>();

            // Automapper
            services.AddAutoMapper();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddMvc().AddMvcOptions(x => x.Filters.Add(new GlobalExceptionFilter()));


            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));

            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));



            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(jwtBearerOptions =>
           {
               jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters()
               {
                   ValidateActor = true,
                   ValidateAudience = true,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   ValidIssuer = Configuration["Issuer"],
                   ValidAudience = Configuration["Audience"],
                   IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SigninKey"]))
               };
           });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new Info
                    {
                        Title = "Tele Billing API",
                        Version = "v1",
                    });

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"

                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", new string[] { } }
                });
            });



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            //Enable middleware to serve swagger - ui(HTML, JS, CSS, etc.), 
            // specifying the Swagger JSON endpoint.
            if (env.IsProduction() || env.IsStaging())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/Applications/telebilling/telebillingAPI/swagger/v1/swagger.json", "Tele Billing API");
                    c.RoutePrefix = string.Empty; //"swagger";
                    c.InjectStylesheet("/Applications/telebilling/telebillingAPI/swagger/custom.css");
                });
            }
            else if (env.IsDevelopment())
            {
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tele Billing API");
                    c.RoutePrefix = string.Empty; //"swagger";
                    c.InjectStylesheet("/swagger/custom.css");
                });
            }

            app.UseStaticFiles();
            app.UseCors("CORS");
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
