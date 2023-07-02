using OpenAI.ChatGpt.Modules.StructuredResponse;

namespace OpenAI.ChatGpt.IntegrationTests.OpenAiClientTests;

public class OpenAiClientGetAsObjectTests
{
    private readonly OpenAiClient _client;

    public OpenAiClientGetAsObjectTests()
    {
        _client = new OpenAiClient(Helpers.GetOpenAiKey());
    }

    [Fact]
    public async void Get_simple_structured_response_from_ChatGPT()
    {
        var message = 
            Dialog.StartAsSystem("What did user input?")
                .ThenUser("My name is John, my age is 30, my email is john@gmail.com");
        var response = await _client.GetStructuredResponse<UserInfo>(message);
        response.Should().NotBeNull();
        response.Name.Should().Be("John");
        response.Age.Should().Be(30);
        response.Email.Should().Be("john@gmail.com");
    }
    
    [Fact]
    public async void Get_structured_response_with_array_from_ChatGPT()
    {
        var message = 
            Dialog.StartAsSystem("What did user input?")
                .ThenUser("My name is John, my age is 30, my email is john@gmail.com. I want to buy 2 apple and 3 orange.");
        var response = await _client.GetStructuredResponse<Order>(message);
        response.Should().NotBeNull();
        response.UserInfo.Name.Should().Be("John");
        response.UserInfo.Age.Should().Be(30);
        response.UserInfo.Email.Should().Be("john@gmail.com");
        
        response.Items.Should().HaveCount(2);
        response.Items[0].Name.Should().Be("apple");
        response.Items[0].Quantity.Should().Be(2);
        response.Items[1].Name.Should().Be("orange");
        response.Items[1].Quantity.Should().Be(3);
    }
    
    private class Order
    {
        public UserInfo UserInfo { get; set; }
        public List<Item> Items { get; set; }

        public class Item
        {
            public string Name { get; set; } = "";
            public int Quantity { get; set; }
        }
    }
    
    private class UserInfo
    {
        public string Name { get; init; }
        public int Age { get; init; }
        public string Email { get; init; }
    }
}
