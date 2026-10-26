using ChatBot;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

Utilities.SetEnvironmentVariable();

var provider = args.Length > 0 ? args[0].ToLower() : "gemini";
var defaultModel = provider switch
{
    "gemini" => "gemini-3.5-flash-lite",
    _ => throw new ArgumentException("Unsupported provider")
};
var model = args.Length > 1 ? args[1] : defaultModel;
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine($"Using provider: {provider}, model: {model}");
Console.ResetColor();

Console.WriteLine();
Console.WriteLine("Welcome to the ChatBot! Type 'exit' to quit.");
Console.WriteLine("You can ask me anything, and I'll do my best to respond.");
Console.WriteLine();

var builder = Host.CreateApplicationBuilder(args);

Startup.ConfigureServices(builder, provider, model);

var app = builder.Build();

var chatClient = app.Services.GetRequiredService<IChatClient>();
await Gemini.Run(chatClient);