using Google.GenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                    option.MaxOutputTokens = 20;
                    option.Temperature = 0.7f;
                })
                .Build(sp);

            });
        }
    }
}
