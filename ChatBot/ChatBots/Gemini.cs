using ChatBot;
using ChatBot.Services;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using System.Text;
using Environment = System.Environment;

internal class Gemini
{
    
    public static async Task Run(IChatClient _client)
    {
        
        MessageHandler chat = new MessageHandler(_client);
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

    private readonly List<ChatMessage> _history = new();
    private readonly IChatClient _client;
  
    public MessageHandler(IChatClient client)
    {
        _client = client;
  
        _history.Add(new ChatMessage { 
            Role = ChatRole.User, 
            Contents = [
                new TextContent("""
        You are an assistant that makes very short and concise responses.
        You must answer in a single sentence and avoid unnecessary details.
        """)
                
                ]
        });

        _history.Add(new ChatMessage
        {
            Role = ChatRole.Assistant,
            Contents = new List<AIContent>
            {
                new TextContent("Understood. I will keep all responses concise.")
            }
        });
    }
             
    public async Task<string> SendMessage(string userMessage)
    {

       try
        {
            _history.Add(
           new ChatMessage
           {
               Role = ChatRole.User,
               Contents = new List<AIContent>
               {
                   new TextContent(userMessage)
               }
           });

            var sb = new StringBuilder();
            
            await foreach (var update in _client.GetStreamingResponseAsync(_history))
            {
                foreach (var content in update.Contents)
                {
                    if (content is TextContent textContent)
                    {
                        sb.Append(textContent.Text);
                    }
                }
            }

            _history.Add(
                new ChatMessage
                {
                    Role = ChatRole.Assistant,
                    Contents = new List<AIContent>
                    {
                        new TextContent(sb.ToString())
                    }
                });

            return sb.ToString() ?? "Sorry, I didn't understand that.";
        }
        catch (ClientError ex)
        {
            return HandleGeminiError(ex);
        }
    }

    private static string HandleGeminiError(ClientError ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Apologies, we couldn't fulfill your request due to an error with the AI service.");
        Console.WriteLine($"Error Details: {ex.Message}");
        Console.ResetColor();
        if (ex.Message.Contains("quota",
            StringComparison.OrdinalIgnoreCase))
        {
            return "Token quota has been exhausted. Please try again later.";
        }

        return "An AI service error occurred.";
    }
}
