using OpenAI.ChatGpt.Modules.StructuredResponse;

namespace OpenAI.ChatGpt.IntegrationTests.OpenAiClientTests;

public class OpenAiClientGetStructuredResponseTests
{
    private readonly OpenAiClient _client = new(Helpers.GetOpenAiKey());

    [Theory]
    [InlineData(ChatCompletionModels.Gpt3_5_Turbo)]
    [InlineData(ChatCompletionModels.Gpt4Turbo)]
    [InlineData(ChatCompletionModels.Gpt4)]
    [InlineData(ChatCompletionModels.Gpt3_5_Turbo_1106)]
    public async void Get_simple_structured_response_from_ChatGPT(string model)
    {
        var message =
            Dialog.StartAsSystem("What did user input?")
                .ThenUser("My name is John, my age is 30, my email is john@gmail.com");
        var examples = new []{ new UserInfo() {Age = 0, Email = "i@rodion-m.ru", Name = "Rodion"} };
        var response = await _client.GetStructuredResponse<UserInfo>(message, model: model, examples: examples);
        response.Should().NotBeNull();
        response.Name.Should().Be("John");
        response.Age.Should().Be(30);
        response.Email.Should().Be("john@gmail.com");
    }

    [Theory]
    [InlineData(ChatCompletionModels.Gpt4Turbo)]
    [InlineData(ChatCompletionModels.Gpt4)]
    public async void Get_structured_response_with_ARRAY_from_ChatGPT(string model)
    {
        var message = Dialog
            .StartAsSystem("What did user input?")
            .ThenUser("My name is John, my age is 30, my email is john@gmail.com. " +
                      "I want to buy 2 apple and 3 orange.");
        var response = await _client.GetStructuredResponse<Order>(message, model: model);
        response.Should().NotBeNull();
        response.UserInfo.Should().NotBeNull();
        response.UserInfo!.Name.Should().Be("John");
        response.UserInfo.Age.Should().Be(30);
        response.UserInfo.Email.Should().Be("john@gmail.com");

        response.Items.Should().HaveCount(2);
        response.Items![0].Name.Should().Be("apple");
        response.Items[0].Quantity.Should().Be(2);
        response.Items[1].Name.Should().Be("orange");
        response.Items[1].Quantity.Should().Be(3);
    }

    [Theory]
    [InlineData(ChatCompletionModels.Gpt4Turbo)]
    [InlineData(ChatCompletionModels.Gpt4)]
    public async void Get_structured_response_with_ENUM_from_ChatGPT(string model)
    {
        var message = Dialog
            .StartAsSystem("What did user input?")
            .ThenUser("Мой любимый цвет - красный");
        var response = await _client.GetStructuredResponse<Thing>(message, model: model);
        response.Should().NotBeNull();
        response.Color.Should().Be(Thing.Colors.Red);
    }

    [Theory]
    [InlineData(ChatCompletionModels.Gpt4Turbo)]
    [InlineData(ChatCompletionModels.Gpt4)]
    public async void Get_structured_response_with_extra_data_from_ChatGPT(string model)
    {
        var message = Dialog
            .StartAsSystem("Return requested data.")
            .ThenUser("I need info about Almaty city");
        var response = await _client.GetStructuredResponse<City>(message, model: model);
        response.Should().NotBeNull();
        response.Name.Should().Be("Almaty");
        response.Country.Should().Be("Kazakhstan");
        response.YearOfFoundation.Should().Be(1854);
    }

    private class Order
    {
        public UserInfo? UserInfo { get; set; }
        public List<Item>? Items { get; set; }

        public record Item(string Name, int Quantity);
    }

    private class UserInfo
    {
        public string? Name { get; init; }
        public int Age { get; init; }
        public string? Email { get; init; }
    }

    private record Thing(Thing.Colors Color)
    {
        public enum Colors
        {
            None,
            Red,
            Green,
            Blue
        }
    }

    private record City(string Name, int YearOfFoundation, string Country);
}