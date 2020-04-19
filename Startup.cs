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
using ClerkBot.Elastic;
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

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Configure state
            ConfigureState(services);
            //services.AddSingleton<IStorage, MemoryStorage>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            //services.AddTransient<IBot, EchoBot>();
            services.AddTransient<IBot, ListBot>();
        }

        public void ConfigureState(IServiceCollection services)
        {
            // Create the storage we'll be using for User and Conversation state. (Memory is great for testing purposes)
            //services.AddSingleton<IStorage, MemoryStorage>();
            services.AddSingleton<IStorage, ElasticsearchStorage>();

            // Conversation State Storage
            var conversationDataStorageOptions = new ElasticsearchStorageOptions
            {
                ElasticsearchEndpoint = new Uri("http://localhost:9200"),
                IndexName = "conversation-data",
                IndexMappingDepthLimit = 100000
            };

            IStorage conversationDataStore = new ElasticsearchStorage(conversationDataStorageOptions);
            var conversationState = new ConversationState(conversationDataStore);
            services.AddSingleton(conversationState);

            //// User State Storage
            var userDataStorageOptions = new ElasticsearchStorageOptions
            {
                ElasticsearchEndpoint = new Uri("http://localhost:9200"),
                IndexName = "user-data",
                IndexMappingDepthLimit = 100000
            };

            IStorage userDataStore = new ElasticsearchStorage(userDataStorageOptions);
            var userState = new UserState(userDataStore);
            services.AddSingleton(userState);
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

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            // Runs matching. An endpoint is selected and set on the HttpContext if a match is found.
            app.UseRouting();

            // Middleware that run after routing occurs. Usually the following appear here:
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();
            // These middleware can take different actions based on the endpoint.

            // Executes the endpoint that was selected by routing.
            app.UseEndpoints(endpoints =>
            {
                // Mapping of endpoints goes here:
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });

            // Middleware here will only run if nothing was matched.
        }
    }
}
