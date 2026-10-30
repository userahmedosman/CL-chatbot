using ChatBot.Services;
using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBot
{
    internal static class Startup
    {
        public static void ConfigureServices(HostApplicationBuilder builder, string provider, string? model)
        {
            var geminiKey = Environment.GetEnvironmentVariable("GEMINI_KEY");
            builder.Services.AddTransient<IWeatherService, WeatherAPIService>();
            builder.Services.AddTransient<EvaluateWeatherConditions>();

            builder.Services.AddTransient<EmailGetService>();
            builder.Services.AddTransient<EmailSendService>();

            builder.Services.AddHttpClient();
            builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.None);
            builder.Services.AddChatClient(sp =>
            {
                var client = provider switch 
                {
                    "gemini" => new  Client(apiKey: geminiKey).AsIChatClient(model),
                    _ => throw new ArgumentException("Unsupported provider")
                };

                return client
                .AsBuilder()
                .ConfigureOptions(option =>
                {
                  
                    option.Tools = [.. Tools.GetTools(sp)];

                    
                })
                .UseFunctionInvocation()
                .Build(sp);

            });
        }
    }
}
