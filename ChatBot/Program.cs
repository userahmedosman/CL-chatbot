using ChatBot;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using Environment = System.Environment;

 internal class Program
{
    public static async Task Main(string[] args)
    {

        Utilities.SetEnvironmentVariable();
        var gemini = "gemini-3.5-flash-lite";

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Available Model:");
        Console.ResetColor();


        Console.WriteLine(gemini);


        Console.WriteLine("Welcome to the ChatBot! Type 'exit' to quit.");
        Console.WriteLine("You can ask me anything, and I'll do my best to respond.");
        Console.WriteLine();
        MessageHandler Program = new MessageHandler();
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
            
            var response = await Program.SendMessage(userInput);

            Console.WriteLine(response);
            Console.WriteLine();
        }
    }

}

internal class MessageHandler
{

    private readonly List<Content> _history = new();
    public async Task<string> SendMessage(string userMessage)
    {
        var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var client = new Client(apiKey: geminiApiKey);
        var info = "make a very short and consice responces";

        _history.Add(
            new Content
            {
                Role = "user",
                Parts = [Part.FromText("Instruction: " + info + "Question: " + userMessage)]
            });

        var response = await client.Models.GenerateContentAsync(
            model: "gemini-3.5-flash",
            contents: _history);

        _history.Add(
            new Content
            {
                Role = "model",
                Parts = [Part.FromText(response.Text)]
            });

        return response?.Text ?? "Sorry, I didn't understand that.";
    }
}
