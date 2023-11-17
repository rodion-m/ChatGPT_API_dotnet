namespace OpenAI.ChatGpt.IntegrationTests;

public class ChatGptTests
{
    [Fact]
    public async void Stream_chatgpt_response_cancellation_throws_exception()
    {
        ChatService chatService = await CreateInMemoryChat();
        const string text = "Write numbers from 1 to 50";
        await FluentActions.Invoking(
                async () =>
                {
                    await foreach (var _ in chatService.StreamNextMessageResponse(text))
                    {
                        chatService.Stop();
                    }
                })
            .Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    public async void Stream_chatgpt_response_cancellation_with_throwOnCancellation_false_stopped_silently()
    {
        ChatService chatService = await CreateInMemoryChat();
        const string text = "Write numbers from 1 to 50";
        await FluentActions.Invoking(
                async () =>
                {
                    await foreach (var _ in chatService.StreamNextMessageResponse(text, throwOnCancellation: false))
                    {
                        chatService.Stop();
                    }
                })
            .Should().NotThrowAsync();
    }

    private static async Task<ChatService> CreateInMemoryChat()
    {
        return await ChatGPT.CreateInMemoryChat(Helpers.GetOpenAiKey(),
            new ChatGPTConfig()
            {
                MaxTokens = 100
            });
    }
}