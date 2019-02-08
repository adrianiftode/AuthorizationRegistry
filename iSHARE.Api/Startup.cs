﻿using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using iSHARE.Api.ApplicationInsights;
using iSHARE.Api.Configurations;
using iSHARE.Api.Convertors;
using iSHARE.Api.Filters;
using iSHARE.Api.Swagger;
using iSHARE.Configuration;
using iSHARE.Models;

namespace iSHARE.Api
{
    public class Startup
    {
        protected internal string SwaggerName;
        protected internal string ApplicationInsightsName;
        protected internal string SpaScope;

        public Startup(IHostingEnvironment env, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            HostingEnvironment = env;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IMvcCoreBuilder MvcCoreBuilder { get; private set; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            services.ConfigurePartyDetailsOptions(Configuration, HostingEnvironment);
            services.AddSigning();


            MvcCoreBuilder = services.AddMvcCore()
                .AddApiExplorer() //see https://github.com/domaindrivendev/Swashbuckle.AspNetCore#swashbuckle--apiexplorer
                .AddJsonFormatters()
                .AddAuthorization()
                .AddDataAnnotations()
                .AddJsonOptions(options => { options.SerializerSettings.Converters.Add(new TrimmingConverter()); })
                ;

            var scopes = new List<string> { StandardScopes.iSHARE };
            if (!string.IsNullOrEmpty(SpaScope))
            {
                scopes.Add(SpaScope);
            }

            services.AddJwtAuthentication(HostingEnvironment, Configuration);
            services.AddSwagger(SwaggerName, HostingEnvironment);
            services.AddJsonSchema();
            services.AddFileProvider(HostingEnvironment);
            services.AddApplicationInsights(ApplicationInsightsName, HostingEnvironment);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLoggers(Configuration);
            app.UseCors(builder =>
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
            );
            app.UseExceptionHandler(HostingEnvironment);
            app.UseSwagger(SwaggerName);
            app.UseMvc();
        }
    }
}