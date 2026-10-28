using ChatBot.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChatBot
{
    internal static class Tools
    {
        internal static IEnumerable<AITool> GetTools(this IServiceProvider sp)
        {
            var weatherService = sp.GetRequiredService<IWeatherService>();

            yield return AIFunctionFactory.Create(
                weatherService.GetWeatherAsync,
                new AIFunctionFactoryOptions
                {
                    Name = "get_weather",
                    Description = "Gets the current weather conditions for a specified city.",
                    
                });
        }
    }
}
