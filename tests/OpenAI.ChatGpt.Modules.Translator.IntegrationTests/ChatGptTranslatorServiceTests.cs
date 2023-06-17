using FluentAssertions;
using OpenAI.Tests.Shared;

namespace OpenAI.ChatGpt.Modules.Translator.IntegrationTests;

public class ChatGptTranslatorServiceTests
{
    [Fact]
    public async Task Translate_without_source_and_target_languages_uses_default_languages()
    {
        // Arrange
        var expectedSourceLanguage = "English";
        var expectedTargetLanguage = "Russian";
        var textToTranslate = "Hello, world!";
        var translatorService = new ChatGPTTranslatorService(
            Helpers.GetOpenAiKey(),
            null,
            defaultSourceLanguage: expectedSourceLanguage,
            defaultTargetLanguage: expectedTargetLanguage);

        // Act
        var translatedText = await translatorService.Translate(textToTranslate);

        // Assert
        translatedText.Should().NotBeNullOrEmpty();
        translatedText.Should().NotBe(textToTranslate);
    
        var englishCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        translatedText.Should().NotContainAny(englishCharacters.Select(c => new string(new []{ c })));
    
        // Check at least some characters are in the Cyrillic script
        translatedText.Any(ch => ch >= 0x0400 && ch <= 0x04FF).Should().BeTrue(); 
    }

}