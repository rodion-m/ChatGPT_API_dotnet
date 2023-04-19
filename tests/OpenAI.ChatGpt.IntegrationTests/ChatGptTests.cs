namespace OpenAI.ChatGpt.IntegrationTests;

public class ChatGptTests
{
    [Fact]
    public async void Stream_chatgpt_response_cancellation_throws_exception()
    {
        Chat chat = await CreateInMemoryChat();
        const string text = "Write numbers from 1 to 50";
        await FluentActions.Invoking(
                async () =>
                {
                    await foreach (var _ in chat.StreamNextMessageResponse(text))
                    {
                        chat.Stop();
                    }
                })
            .Should().ThrowAsync<OperationCanceledException>();
    }
    
    [Fact]
    public async void Stream_chatgpt_response_cancellation_with_throwOnCancellation_false_stopped_silently()
    {
        Chat chat = await CreateInMemoryChat();
        const string text = "Write numbers from 1 to 50";
        await FluentActions.Invoking(
                async () =>
                {
                    await foreach (var _ in chat.StreamNextMessageResponse(text, throwOnCancellation: false))
                    {
                        chat.Stop();
                    }
                })
            .Should().NotThrowAsync();
    }

    private static async Task<Chat> CreateInMemoryChat()
    {
        return await ChatGPT.CreateInMemoryChat(Helpers.GetKeyFromEnvironment("OPENAI_API_KEY"),
            new ChatGPTConfig()
            {
                MaxTokens = 100
            });
    }
}