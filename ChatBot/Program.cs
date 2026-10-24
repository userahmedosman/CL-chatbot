using ChatBot;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using System.Reflection.Metadata;
using System.Text;
using Environment = System.Environment;

 internal class Program
{
    public static async Task Main(string[] args)
    {

        Utilities.SetEnvironmentVariable();
        var gemini = "gemini-3.5-flash-lite";

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("[Available Model: ");
        Console.ResetColor();


        Console.WriteLine(gemini + "]");
        Console.WriteLine();

        Console.WriteLine("Welcome to the ChatBot! Type 'exit' to quit.");
        Console.WriteLine("You can ask me anything, and I'll do my best to respond.");
        Console.WriteLine();
        MessageHandler chat = new MessageHandler();
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You: ");
            var userInput = Console.ReadLine();
            Console.ResetColor();

            if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit")
            {
                Console.WriteLine("Exiting the ChatBot. Goodbye!");
                break;
            }
            
            var response = await chat.SendMessage(userInput);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Bot: ");
            Console.ResetColor();
            foreach (char c in response)
            {
                Console.Write(c);
                await Task.Delay(34);
            }

            Console.WriteLine();
        }
    }

}

internal class MessageHandler
{

    private readonly List<Content> _history = new();
    private readonly Client _client;
    const string ModelName = "gemini-3.5-flash";
    public MessageHandler()
    {

        var geminiApiKey =
        Environment.GetEnvironmentVariable("GEMINI_API_KEY");

        if (string.IsNullOrWhiteSpace(geminiApiKey))
        {
            throw new InvalidOperationException(
                "GEMINI_API_KEY is not configured.");
        }

        _client = new Client(apiKey: geminiApiKey);

        _history.Add(new Content
        {
            Role = "user",
            Parts =
    [
        Part.FromText("""
        You are an assistant that makes very short and concise responses.
        You must answer in a single sentence and avoid unnecessary details.
        """)
    ]
        });

        _history.Add(new Content
        {
            Role = "model",
            Parts =
            [
                Part.FromText("Understood. I will keep all responses concise.")
            ]
        });
    }
     
    public async Task<string> SendMessage(string userMessage)
    {

        _history.Add(
            new Content
            {
                Role = "user",
                Parts = [Part.FromText(userMessage)]
            });

        var sb = new StringBuilder();
        var stream = _client.Models.GenerateContentStreamAsync(
            model: ModelName, contents: _history);

        await foreach (var part in stream)
        {
            foreach (var content in part?.Parts)
            {
                if (content?.Text != null)
                {
                    sb.Append(content.Text);
                   
                }
            }
        }

        _history.Add(
            new Content
            {
                Role = "model",
                Parts = [Part.FromText(sb.ToString())]
            });

        return sb.ToString() ?? "Sorry, I didn't understand that.";
    }
}
