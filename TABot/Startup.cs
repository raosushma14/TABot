﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.5.0

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TABot.Bots;
using TABot.Bots.Dialogs;
using TABot.Services.BotServices;
using TABot.Services.EmailServices;
using TABot.Services.TableStorageService;

namespace TABot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            services.AddSingleton<IStorage, MemoryStorage>();

            services.AddSingleton<UserState>();

            services.AddSingleton<ConversationState>();

            services.AddSingleton<IBotServices, BotServices>();

            services.AddTransient<TableStorageService>((serviceProvider) =>
            {
                return new TableStorageService(Configuration["StorageConnectionString"]);
            });

            //Register email service as a transient dependency in the IOC
            services.AddTransient<EmailService>((serviceProvider) => {
                var baseUrl = Configuration["SendGridBaseUrl"];
                var authKey = Configuration["SendGridAuthKey"];
                var fromEmail = Configuration["SendGridFromEmail"];
                var toEmail = Configuration["SendGridToEmail"];

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(authKey))
                {
                    throw new InvalidOperationException("email Base URL or auth key is Missing. Please add your base url to the 'sendGridBaseUrl' setting.");
                }
                return new EmailService(baseUrl, fromEmail, toEmail, authKey);
            });

            services.AddTransient<ComputerVisionClient>((serviceProvider) => {
                return new ComputerVisionClient(
                    new ApiKeyServiceClientCredentials(Configuration["ComputerVisionSubscriptionKey"]))
                {
                    Endpoint = Configuration["ComputerVisionEndPoint"]                    
                };
            });

            services.AddSingleton<ErrorEnquiryDialog>();
            services.AddSingleton<MainDialog>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, DialogAndWelcomeBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
