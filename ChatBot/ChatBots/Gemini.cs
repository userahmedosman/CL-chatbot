using ChatBot;
using ChatBot.Services;
using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.Extensions.AI;
using System.Text;
using static Google.Apis.Requests.BatchRequest;
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
            Console.ResetColor();
            var userInput = Console.ReadLine();
           

            if (string.IsNullOrEmpty(userInput) || userInput.ToLower() == "exit")
            {
                Console.WriteLine("Exiting the ChatBot. Goodbye!");
                break;
            }
            
            await chat.SendMessage(userInput);

            
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
             
    public async Task SendMessage(string userMessage)
    {
        while (true)
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

                var updates = new List<ChatResponseUpdate>();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("BOT: ");
                Console.ResetColor();
                await foreach (var fragment in _client.GetStreamingResponseAsync(_history))
                {
                    updates.Add(fragment);

                    foreach (var message in fragment.Contents)
                    {
                       
                        if (message is TextContent textContent)
                        {
                           
                            foreach (char c in textContent.Text)
                            {
                                Console.Write(c);
                                await Task.Delay(34);
                            }
                            

                        }
                    }
                }

                var responce = updates.ToChatResponse();

                _history.AddMessages(responce);

                var approvalRequest = responce.Messages
                    .SelectMany(m => m.Contents)
                    .OfType<ToolApprovalRequestContent>()
                    .FirstOrDefault();

                if (approvalRequest != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"\nApproval Request:");
                    Console.ResetColor();
                    Console.Write("Do you approve this action? ");
                    if (approvalRequest.ToolCall is FunctionCallContent functionCall)
                    {
                        Console.WriteLine($"Function: {ConvertToolName(functionCall.Name)}");
                        if (functionCall.Arguments is not null)
                        {
                            foreach (var arg in functionCall.Arguments)
                            {
                                Console.WriteLine($"Argument: {arg.Key} = {arg.Value}");
                            }
                        }
                    }
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Your response here [yes / no]: ");
                    Console.ResetColor();
                    var approvalInput = Console.ReadLine()?.Trim().ToLower();

                    var approvedResponce = approvalRequest.CreateResponse(approvalInput == "yes" || approvalInput == "y");

                    _history.Add(
               new ChatMessage
               {
                   Role = ChatRole.User,
                   Contents = new List<AIContent>
                   {
                   approvedResponce
                   }
               });
                    Console.WriteLine();
                    continue;

                }
                Console.WriteLine();
                Console.WriteLine();
                break;


            }
            catch (ClientError ex)
            {
                HandleGeminiError(ex);
            }
        }
    }

    private static string ConvertToolName(string toolName)
    {
        return toolName switch
        {
            "get_weather" => "Get Weather",
            "evaluate_weather" => "Evaluate Weather",
            "send_email" => "Send Email",
            _ => toolName
        };
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
