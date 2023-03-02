using System.Text;
using FluentAssertions;
using OpenAI.Models.ChatCompletion;
using OpenAI.Models.Images;
using Xunit.Abstractions;

namespace OpenAI.Test;

public class OpenAiClientTests
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly OpenAiClient _client;

    public OpenAiClientTests(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _client = new OpenAiClient(Helpers.GetKeyFromEnvironment("openai_api_key_paid"));
    }

    [Fact]
    public async void Get_chatgpt_response_for_one_message_works()
    {
        string text = "Who are you?";
        string response = await _client.GetChatCompletions(new UserMessage(text), 80);
        response.Should().NotBeNullOrEmpty();
        _outputHelper.WriteLine(response);
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
        ChatCompletionDialog messages = 
            new UserMessage("How many meters are in a kilometer? Write just the number.")
            .ThenAssistant("1000")
            .ThenUser("Convert it to hex. Write just the number.");

        var sb = new StringBuilder();
        await foreach (var chunk in _client.StreamChatCompletions(messages, 80))
        {
            sb.Append(chunk);
        }
        var answer = sb.ToString();
        _outputHelper.WriteLine(answer);
        answer.ToUpper().Should().Contain("3E8");
    }

    [Fact]
    public async void Generate_image_bytes_works()
    {
        byte[] image = await _client.GenerateImageBytes("bicycle", "test", OpenAiImageSize._256);
        image.Should().NotBeEmpty();
    }
    
    [Fact]
    public async void Generate_two_images_uri_works()
    {
        Uri[] uris = await _client.GenerateImagesUris("bicycle", "test", OpenAiImageSize._256, count: 2);
        uris.Should().HaveCount(2);
    }
}