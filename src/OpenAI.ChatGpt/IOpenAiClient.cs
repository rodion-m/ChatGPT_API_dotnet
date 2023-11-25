namespace OpenAI.ChatGpt;

/// <summary>
///  The OpenAI client interface.
/// </summary>
[Obsolete($"Will be removed in the next major version. Use {nameof(IAiClient)} instead.")]
public interface IOpenAiClient : IAiClient;