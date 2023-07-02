using FluentAssertions;
using Moq;
using OpenAI.ChatGpt.Models.ChatCompletion;
using OpenAI.ChatGpt.Models.ChatCompletion.Messaging;

namespace OpenAI.ChatGpt.Modules.Translator.UnitTests;

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
    public void Dispose_with_injected_client_should_not_dispose_client()
    {
        // Arrange
        var clientMock = new Mock<OpenAiClient>();
        clientMock.Setup(client => client.Dispose()).Verifiable();
        var translatorService = new ChatGPTTranslatorService(clientMock.Object);

        // Act
        translatorService.Dispose();

        // Assert
        clientMock.Verify(client => client.Dispose(), Times.Never);
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
            .ReturnsAsync("Привет, мир!")
            .Verifiable();
        var translatorService = new ChatGPTTranslatorService(
            clientMock.Object, 
            defaultSourceLanguage: expectedSourceLanguage, 
            defaultTargetLanguage: expectedTargetLanguage);

        // Act
        var translatedText = await translatorService.Translate(textToTranslate);

        // Assert
        clientMock.Verify(client => client.GetChatCompletions(
                It.Is<UserOrSystemMessage>(dialog => 
                    dialog.GetMessages().Any(
                        message => message.Role == "system" && 
                                   message.Content.Contains($"I want you to act as a translator from {expectedSourceLanguage} to {expectedTargetLanguage}"))),
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<float>(), 
                It.IsAny<string>(), 
                It.IsAny<Action<ChatCompletionRequest>>(),
                It.IsAny<Action<ChatCompletionResponse>>(), 
                It.IsAny<CancellationToken>()), 
            Times.Once);
        translatedText.Should().Be("Привет, мир!");
    }
}