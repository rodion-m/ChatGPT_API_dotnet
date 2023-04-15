using System.Text;
using FluentAssertions;
using OpenAI.Models.ChatCompletion;
using Xunit.Abstractions;

namespace OpenAI.Core.Test;

[Collection("OpenAiTestCollection")] //to prevent parallel execution
public class ChatCompletionsApiTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly OpenAiClient _client;

    public ChatCompletionsApiTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _client = new OpenAiClient(Helpers.GetKeyFromEnvironment("openai_api_key_paid"));
    }

    [Fact]
    public async void Get_chatgpt_response_for_one_message_works()
    {
        string text = "Who are you?";
        string response = await _client.GetChatCompletions(new UserMessage(text), 80);
        _outputHelper.WriteLine(response);
        response.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async void Get_chatgpt_response_for_one_message_including_system_works()
    {
        var message = 
            Dialog.StartAsSystem("You are a helpful assistant that translates English to French.")
                .ThenUser("Translate 'Hello'. Write just one word.");
        string response = await _client.GetChatCompletions(message, 80);
        _outputHelper.WriteLine(response);
        response.Should().StartWith("Bonjour");
    }
    
    [Fact]
    public async void Dialog_started_from_assistant_works()
    {
        var message = 
            new AssistantMessage("How are your today?")
                .ThenUser("I'm fine, thanks.");
        string response = await _client.GetChatCompletions(message, 80);
        _outputHelper.WriteLine(response);
        response.Should().NotBeEmpty();
    }
    
    [Fact]
    public async void Stream_chatgpt_response_for_one_message_works()
    {
        var text = "Write the world top 3 songs of Soul genre";
        var sb = new StringBuilder();
        await foreach (string chunk in _client.StreamChatCompletions(new UserMessage(text), 80))
        {
            sb.Append(chunk);
            chunk.Should().NotBeNull();
        }
        _outputHelper.WriteLine(sb.ToString());
    }
    
    [Fact]
    public async void Stream_chatgpt_response_for_dialog_works()
    {
        var dialog = 
            Dialog.StartAsUser("How many meters are in a kilometer? Write just the number.")
                .ThenAssistant("1000")
                .ThenUser("Convert it to hex. Write just the number.")
            ;

        var sb = new StringBuilder();
        await foreach (var chunk in _client.StreamChatCompletions(dialog, 80))
        {
            sb.Append(chunk);
        }
        var answer = sb.ToString();
        _outputHelper.WriteLine(answer);
        answer.ToUpper().Should().Contain("3E8");
    }
}