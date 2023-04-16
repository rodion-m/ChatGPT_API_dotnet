namespace OpenAI.ChatGpt.UnitTests.MessageStorage;

public abstract class AbstractMessageStorageTests
{
    private readonly IMessageStorage _messageStorage;

    public AbstractMessageStorageTests(IMessageStorage messageStorage)
    {
        _messageStorage = messageStorage ?? throw new ArgumentNullException(nameof(messageStorage));
    }

    [Fact]
    public async Task Save_messages_for_user_and_topic()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        var now = DateTimeOffset.Now;
        IEnumerable<PersistentChatMessage> messages = CreateMessages(userId, topicId, now);
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        await _messageStorage.SaveMessages(userId, topicId, messages, cancellationToken);

        // Assert
        var savedMessages = await _messageStorage.GetMessages(userId, topicId, cancellationToken);
        savedMessages.Should().BeEquivalentTo(messages);
    }

    private static List<PersistentChatMessage> CreateMessages(string userId, Guid topicId,
        DateTimeOffset now)
    {
        return new List<PersistentChatMessage>
        {
            new(Guid.NewGuid(), userId, topicId, now, ChatCompletionRoles.User, "Test message 1"),
            new(Guid.NewGuid(), userId, topicId, now, ChatCompletionRoles.Assistant,
                "Test message 2"),
        };
    }

    [Fact]
    public async Task Save_messages_with_null_messages_is_invalid()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = async () =>
            await _messageStorage.SaveMessages(userId, topicId, null, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*messages*");
    }

    [Fact]
    public async Task Save_messages_with_null_user_id_is_invalid()
    {
        // Arrange
        string userId = null;
        Guid topicId = Guid.NewGuid();
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = async () =>
            await _messageStorage.SaveMessages(userId, topicId, null, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*userId*");
    }

    [Fact]
    public async Task Save_messages_respects_cancellation_token()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        IEnumerable<PersistentChatMessage> messages =
            CreateMessages(userId, topicId, DateTimeOffset.Now);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        CancellationToken cancellationToken = cts.Token;

        // Act
        Func<Task> act = async () =>
            await _messageStorage.SaveMessages(userId, topicId, messages, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Get_messages_for_user_and_topic()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        IEnumerable<PersistentChatMessage> messages =
            CreateMessages(userId, topicId, DateTimeOffset.Now);
        CancellationToken cancellationToken = new CancellationToken();
        await _messageStorage.SaveMessages(userId, topicId, messages, cancellationToken);

        // Act
        var retrievedMessages =
            await _messageStorage.GetMessages(userId, topicId, cancellationToken);

        // Assert
        retrievedMessages.Should().BeEquivalentTo(messages);
    }

    [Fact]
    public async Task Get_messages_with_null_user_id_is_invalid()
    {
        // Arrange
        string userId = null;
        Guid topicId = Guid.NewGuid();
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => _messageStorage.GetMessages(userId, topicId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*userId*");
    }

    [Fact]
    public async Task Get_messages_respects_cancellation_token()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        // Act
        Func<Task> act = () => _messageStorage.GetMessages(userId, topicId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Edit_message_content_for_specific_message_id()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        string originalMessage = "Original message";
        string newMessage = "Edited message";
        CancellationToken cancellationToken = new CancellationToken();
        IEnumerable<PersistentChatMessage> messages = new List<PersistentChatMessage>
        {
            new(Guid.NewGuid(), userId, topicId, DateTimeOffset.Now, ChatCompletionRoles.User,
                "Test message 1"),
            new(messageId, userId, topicId, DateTimeOffset.Now, ChatCompletionRoles.Assistant,
                originalMessage),
        };
        ;
        await _messageStorage.SaveMessages(userId, topicId, messages, cancellationToken);

        // Act
        await _messageStorage.EditMessage(userId, topicId, messageId, newMessage,
            cancellationToken);

        // Assert
        var retrievedMessages =
            await _messageStorage.GetMessages(userId, topicId, cancellationToken);
        retrievedMessages.First(it => it.Id == messageId)
            .Content
            .Should().Be(newMessage);
    }

    [Fact]
    public async Task Edit_message_with_null_user_id_is_invalid()
    {
        // Arrange
        string? userId = null;
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        string newMessage = "Edited message";
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = () => _messageStorage.EditMessage(userId, topicId, messageId, newMessage,
            cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*userId*");
    }

    [Fact]
    public async Task Edit_message_with_null_new_message_is_invalid()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        string? newMessage = null;
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = async () =>
            await _messageStorage.EditMessage(userId, topicId, messageId, newMessage,
                cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*newMessage*");
    }

    [Fact]
    public async Task Edit_message_respects_cancellation_token()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        string newMessage = "Edited message";
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        CancellationToken cancellationToken = cts.Token;

        // Act
        Func<Task> act = async () =>
            await _messageStorage.EditMessage(userId, topicId, messageId, newMessage,
                cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Delete_message_for_user_and_topic()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        CancellationToken cancellationToken = new CancellationToken();
        IEnumerable<PersistentChatMessage> messages = new List<PersistentChatMessage>
        {
            new(Guid.NewGuid(), userId, topicId, DateTimeOffset.Now, ChatCompletionRoles.User,
                "Test message 1"),
            new(messageId, userId, topicId, DateTimeOffset.Now, ChatCompletionRoles.Assistant,
                "Test message 2"),
            new(Guid.NewGuid(), userId, topicId, DateTimeOffset.Now, ChatCompletionRoles.User,
                "Test message 3"),
        };
        await _messageStorage.SaveMessages(userId, topicId, messages, cancellationToken);

        // Act
        bool deleted =
            await _messageStorage.DeleteMessage(userId, topicId, messageId, cancellationToken);

        // Assert
        deleted.Should().BeTrue();
        var retrievedMessages =
            await _messageStorage.GetMessages(userId, topicId, cancellationToken);
        retrievedMessages.Should().NotContain(it => it.Id == messageId);
    }

    [Fact]
    public async Task Delete_message_returns_false_if_message_id_not_found()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        bool deleted =
            await _messageStorage.DeleteMessage(userId, topicId, messageId, default);

        // Assert
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task Delete_message_with_null_user_id_is_invalid()
    {
        // Arrange
        string userId = null;
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        CancellationToken cancellationToken = new CancellationToken();

        // Act
        Func<Task> act = async () =>
            await _messageStorage.DeleteMessage(userId, topicId, messageId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>().WithMessage("*userId*");
    }

    [Fact]
    public async Task Delete_message_respects_cancellation_token()
    {
        // Arrange
        string userId = "testUser";
        Guid topicId = Guid.NewGuid();
        Guid messageId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var cancellationToken = cts.Token;

        // Act
        Func<Task> act = async () =>
            await _messageStorage.DeleteMessage(userId, topicId, messageId, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}