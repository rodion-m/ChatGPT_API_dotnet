using FluentAssertions;
using OpenAI.Tests.Shared;

namespace OpenAI.ChatGpt.Modules.Translator.IntegrationTests;

public class ChatGptTranslatorServiceTests
{
    private readonly IOpenAiClient _client;

    public ChatGptTranslatorServiceTests()
    {
        _client = new OpenAiClient(Helpers.GetOpenAiKey());
    }
    
    [Fact]
    public async Task Translate_from_English_to_Russian()
    {
        // Arrange
        var sourceLanguage = "English";
        var targetLanguage = "Russian";
        var textToTranslate = "Hello, world!";
        // Act
        var translatedText = await _client.TranslateText(textToTranslate, sourceLanguage, targetLanguage);

        // Assert
        translatedText.Should().NotBeNullOrEmpty();
        translatedText.Should().NotBe(textToTranslate);
    
        var englishCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        translatedText.Should().NotContainAny(englishCharacters.Select(c => new string(new []{ c })));
    
        // Check at least some characters are in the Cyrillic script
        translatedText.Any(ch => ch >= 0x0400 && ch <= 0x04FF).Should().BeTrue(); 
    }

    [Fact]
    public async Task Translate_object_from_English_to_Russian()
    {
        // Arrange
        var sourceLanguage = "English";
        var targetLanguage = "Russian";
        var objectToTranslate = new Order(
            Guid.NewGuid(), 
            new List<Order.Item> 
            {
                new(1,"Book", 5),
                new(2,"Pen", 10),
                new(3,"Notebook", 3)
            }
        );

        var englishRussianDictionary = new Dictionary<string, string>
        {
            { "Book", "Книга" },
            { "Pen", "Ручка" },
            { "Notebook", "Тетрадь" }
        };

        // Act
        var translatedObject = await _client.TranslateObject(objectToTranslate, sourceLanguage, targetLanguage);

        // Assert
        translatedObject.Should().NotBeNull();
        translatedObject.Id.Should().Be(objectToTranslate.Id);
        translatedObject.Items.Should().HaveCount(objectToTranslate.Items.Count);

        foreach (var originalItem in objectToTranslate.Items)
        {
            var translatedItem = translatedObject.Items.FirstOrDefault(i => i.Id == originalItem.Id);
            translatedItem.Should().NotBeNull();
            translatedItem!.Name.Should().NotBe(originalItem.Name);
            translatedItem.Quantity.Should().Be(originalItem.Quantity);
        
            // Check the translation
            englishRussianDictionary.TryGetValue(originalItem.Name, out var expectedRussianName);
            translatedItem.Name.Should().Be(expectedRussianName);
        }
    }
    
    private record Order(Guid Id, List<Order.Item> Items)
    {
        public record Item(int Id, string Name, int Quantity);
    }
}