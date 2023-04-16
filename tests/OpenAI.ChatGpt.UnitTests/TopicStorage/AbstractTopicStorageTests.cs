namespace OpenAI.ChatGpt.UnitTests.TopicStorage;

public abstract class AbstractTopicStorageTests
{
    private readonly ITopicStorage _topicStorage;

    public AbstractTopicStorageTests(ITopicStorage topicStorage)
    {
        _topicStorage = topicStorage ?? throw new ArgumentNullException(nameof(topicStorage));
    }

    [Fact]
    public async Task Add_topic_for_user_and_retrieve_it()
    {
        var userId = "user-1";
        var topic = new Topic(Guid.NewGuid(), userId, "New Topic", DateTimeOffset.UtcNow, new ChatCompletionsConfig());

        await _topicStorage.AddTopic(topic, CancellationToken.None);
        var retrievedTopic = await _topicStorage.GetTopic(userId, topic.Id, CancellationToken.None);

        retrievedTopic.Should().BeEquivalentTo(topic);
    }

    [Fact]
    public async Task Get_most_recent_topic_for_user_with_no_topics_returns_null()
    {
        var userId = "user-2";
        var recentTopic = await _topicStorage.GetMostRecentTopicOrNull(userId, CancellationToken.None);

        recentTopic.Should().BeNull();
    }

    [Fact]
    public async Task Edit_topic_name_and_verify_changes()
    {
        var userId = "user-3";
        var topic = new Topic(Guid.NewGuid(), userId, "Old Name", DateTimeOffset.UtcNow, new ChatCompletionsConfig());

        await _topicStorage.AddTopic(topic, CancellationToken.None);
        var newName = "New Name";
        await _topicStorage.EditTopicName(userId, topic.Id, newName, CancellationToken.None);
        var updatedTopic = await _topicStorage.GetTopic(userId, topic.Id, CancellationToken.None);

        updatedTopic.Name.Should().Be(newName);
    }

    [Fact]
    public async Task Delete_topic_and_verify_removal()
    {
        var userId = "user-4";
        var topic = new Topic(Guid.NewGuid(), userId, "Topic to delete", DateTimeOffset.UtcNow, new ChatCompletionsConfig());

        await _topicStorage.AddTopic(topic, CancellationToken.None);
        var deletionResult = await _topicStorage.DeleteTopic(userId, topic.Id, CancellationToken.None);

        deletionResult.Should().BeTrue();
        await FluentActions.Invoking(() => _topicStorage.GetTopic(userId, topic.Id, CancellationToken.None))
            .Should().ThrowAsync<TopicNotFoundException>();
    }

    [Fact]
    public async Task Retrieve_most_recent_topic_for_user_with_multiple_topics()
    {
        var userId = "user-5";
        var topic1 = new Topic(Guid.NewGuid(), userId, "Topic 1", DateTimeOffset.UtcNow.AddMinutes(-5), new ChatCompletionsConfig());
        var topic2 = new Topic(Guid.NewGuid(), userId, "Topic 2", DateTimeOffset.UtcNow.AddMinutes(-2), new ChatCompletionsConfig());
        var topic3 = new Topic(Guid.NewGuid(), userId, "Topic 3", DateTimeOffset.UtcNow.AddMinutes(-10), new ChatCompletionsConfig());

        await _topicStorage.AddTopic(topic1, CancellationToken.None);
        await _topicStorage.AddTopic(topic2, CancellationToken.None);
        await _topicStorage.AddTopic(topic3, CancellationToken.None);

        var recentTopic = await _topicStorage.GetMostRecentTopicOrNull(userId, CancellationToken.None);

        recentTopic.Should().BeEquivalentTo(topic2);
    }
}
