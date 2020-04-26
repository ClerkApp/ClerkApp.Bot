// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.6.2

using System;
using Bot.Storage.Elasticsearch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ClerkBot.Bots;
using ClerkBot.Dialogs;
using ClerkBot.Services;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Hosting;

namespace ClerkBot
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();

            // Create the credential provider to be used with the Bot Framework Adapter.
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Configure Services
            ConfigureAditionalServices(services);

            // Configure State
            ConfigureState(services);

            // Configure Dialogs
            ConfigureDialogs(services);

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, DialogAndWelcomeBot<RootDialog>>();
            services.AddTransient<IBot, AuthBot<RootDialog>>();
        }

        private void ConfigureAditionalServices(IServiceCollection services)
        {
            services.AddSingleton<BotServices>();
            services.AddSingleton<BotStateService>();
        }

        public void ConfigureState(IServiceCollection services)
        {
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes.) 
            services.AddSingleton<IStorage, ElasticsearchStorage>();

            var conversationState = new ConversationState(
                new ElasticsearchStorage(
                    new ElasticsearchStorageOptions
                    {
                        ElasticsearchEndpoint = new Uri(Configuration["ConnectionStrings:Elasticsearch:Endpoint"]),
                        IndexName = Configuration["ConnectionStrings:Elasticsearch:ConversationIndex"]
                    }
                ));

            var userState = new UserState(
                new ElasticsearchStorage(
                    new ElasticsearchStorageOptions
                    {
                        ElasticsearchEndpoint = new Uri(Configuration["ConnectionStrings:Elasticsearch:Endpoint"]),
                        IndexName = Configuration["ConnectionStrings:Elasticsearch:UserIndex"]
                    }
                ));

            services.AddSingleton(conversationState);
            services.AddSingleton(userState);
        }

        public void ConfigureDialogs(IServiceCollection services)
        {
            services.AddSingleton<RootDialog>();
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
                app.UseHsts();
            }

            app.UseCors();
            //app.UseHttpsRedirection();
            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapRazorPages();
                    //endpoints.MapBlazorHub();
                    //endpoints.MapFallbackToPage("/_Host");
                    endpoints.MapHealthChecks("/health");
                });
        }
    }
}
