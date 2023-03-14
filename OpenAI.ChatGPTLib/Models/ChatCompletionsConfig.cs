using OpenAI.Models.ChatCompletion;

namespace OpenAI.ChatCompletions.Chat.Models;

public class ChatCompletionsConfig
{
    private int? _maxTokens;

    public int? MaxTokens
    {
        get => _maxTokens;
        set
        {
            if (value is <= 0 or > ChatCompletionRequest.MaxTokensLimit)
                throw new ArgumentOutOfRangeException(
                    nameof(MaxTokens),
                    value,
                    $"Must be greater than 0 and less than {ChatCompletionRequest.MaxTokensLimit}"
                );
            _maxTokens = value;
        }
    }

    public string? Model { get; set; }
    public float? Temperature { get; set; }
    public bool PassUserIdToOpenAiRequests { get; set; } = true;

    internal void ModifyRequest(ChatCompletionRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        
        if (_maxTokens is not null) request.MaxTokens = _maxTokens.Value;
        if (Model is not null) request.Model = Model;
        if (Temperature is not null) request.Temperature = Temperature.Value;
    }
}