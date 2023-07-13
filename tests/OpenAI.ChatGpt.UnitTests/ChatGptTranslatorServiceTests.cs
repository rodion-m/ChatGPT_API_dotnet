using Moq;
using OpenAI.ChatGpt.Modules.Translator;

namespace OpenAI.ChatGpt.UnitTests;

public class ChatGptTranslatorServiceTests
{
    [Fact]
    public void Initialization_with_null_api_key_should_throw_exception()
    {
        // Arrange & Act
        Action act = () => new ChatGPTTranslatorService(
            apiKey: null!, 
            host: "host");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Initialization_with_null_client_should_throw_exception()
    {
        // Arrange & Act
        Action act = () => new ChatGPTTranslatorService(client: null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task Translate_without_source_and_target_languages_uses_default_languages()
    {
        // Arrange
        var expectedSourceLanguage = "English";
        var expectedTargetLanguage = "Russian";
        var textToTranslate = "Hello, world!";
        var clientMock = new Mock<IOpenAiClient>();
        clientMock.Setup(client => client.GetChatCompletions(
                It.IsAny<UserOrSystemMessage>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<float>(), 
                It.IsAny<string>(), 
                It.IsAny<Action<ChatCompletionRequest>>(), 
                It.IsAny<Action<ChatCompletionResponse>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Привет, мир!");
        
        var translatorServiceMock = new Mock<ChatGPTTranslatorService>(
            (IOpenAiClient) clientMock.Object, 
            expectedSourceLanguage, 
            expectedTargetLanguage,
            null);
        
        translatorServiceMock.Setup(service => service.CreateTextTranslationPrompt(
                It.IsAny<string>(), It.IsAny<string>()))
            .Returns($"{expectedSourceLanguage} => {expectedTargetLanguage}")
            .Verifiable();

        var translatorService = translatorServiceMock.Object;

        // Act
        _ = await translatorService.TranslateText(textToTranslate);

        // Assert
        translatorServiceMock.Verify(service => 
                service.CreateTextTranslationPrompt(
                    expectedSourceLanguage, expectedTargetLanguage), 
            Times.Once);
    }
    
    
}