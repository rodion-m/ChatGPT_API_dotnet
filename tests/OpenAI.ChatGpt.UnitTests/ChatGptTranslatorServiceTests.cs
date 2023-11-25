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
        var clientMock = new Mock<IAiClient>();
        clientMock.Setup(client => client.GetChatCompletions(
                It.IsAny<UserOrSystemMessage>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(), 
                It.IsAny<float>(), 
                It.IsAny<string>(), 
                It.IsAny<bool>(), 
                It.IsAny<long?>(), 
                It.IsAny<Action<ChatCompletionRequest>>(), 
                It.IsAny<Action<ChatCompletionResponse>>(), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("Привет, мир!");
        
        var translatorServiceMock = new Mock<ChatGPTTranslatorService>(
            (IAiClient) clientMock.Object, 
            expectedSourceLanguage, 
            expectedTargetLanguage,
            (string) null!);
        
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
    
    [Fact]
    public async Task Translating_multiple_texts_should_be_batched_together()
    {
        // Arrange
        var mockTranslatorService = new Mock<IChatGPTTranslatorService>();
        var batch = new EconomicalChatGPTTranslatorService.Batch();
        batch.Add("Text 1");
        batch.Add("Text 2");

        mockTranslatorService.Setup(m 
                => m.TranslateObject(It.IsAny<EconomicalChatGPTTranslatorService.Batch>(),
                    true,
                    "en",
                    "fr",
                    It.IsAny<int?>(),
                    It.IsAny<string>(),
                    It.IsAny<float>(),
                    null,
                    default,
                    default,
                    default,
                    default,
                    default)
            ).ReturnsAsync(batch)
            .Verifiable();

        var service = new EconomicalChatGPTTranslatorService(
            mockTranslatorService.Object, "en", "fr", maxTokensPerRequest: 50
        );

        // Act
        var result1 = service.TranslateText("Text 1");
        var result2 = service.TranslateText("Text 2");

        // Assert
        await Task.WhenAll(result1, result2);
        mockTranslatorService.Verify(m 
                => m.TranslateObject(It.IsAny<EconomicalChatGPTTranslatorService.Batch>(),
                    true,
                    "en",
                    "fr",
                    It.IsAny<int?>(),
                    It.IsAny<string>(),
                    It.IsAny<float>(),
                    null,
                    default,
                    default,
                    default,
                    default,
                    default),
            Times.Once);
    }

    [Fact]
    public async Task Batch_is_processed_after_inactivity_period()
    {
        var mockTranslatorService = new Mock<IChatGPTTranslatorService>();

        var service = new EconomicalChatGPTTranslatorService(
            mockTranslatorService.Object,
            "en",
            "fr",
            sendRequestAfterInactivity: TimeSpan.FromMilliseconds(100)
        );

        _ = service.TranslateText("Hello");

        await Task.Delay(150); // Wait for the inactivity period

        mockTranslatorService.Verify(x => 
            x.TranslateObject(It.IsAny<EconomicalChatGPTTranslatorService.Batch>(),
                true,
                "en",
                "fr",
                It.IsAny<int?>(),
                It.IsAny<string>(),
                It.IsAny<float>(),
                null,
                default,
                default,
                default,
                default,
                default),
            Times.Once);
    }

}