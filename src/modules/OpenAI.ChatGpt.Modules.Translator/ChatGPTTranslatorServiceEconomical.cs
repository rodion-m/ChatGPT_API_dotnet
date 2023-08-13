using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Modules.Translator;

/// <summary>
/// Provides a service for translating text using GPT models with economical batching.
/// </summary>
public class ChatGPTTranslatorServiceEconomical : IAsyncDisposable
{
    private readonly IChatGPTTranslatorService _chatGptTranslatorService;
    private readonly string _sourceLanguage;
    private readonly string _targetLanguage;
    private readonly int? _maxTokens;
    private readonly string _model;
    private readonly float _temperature;
    private readonly string? _user;
    private readonly int? _maxTokensPerRequest;
    private readonly TimeSpan _sendRequestAfterInactivity;
    private static readonly TimeSpan DefaultSendRequestAfterInactivity = TimeSpan.FromMilliseconds(100);
    
    private Batch _batch = new();
    private TaskCompletionSource<Batch> _tcs = new();
    private readonly object _syncLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ChatGPTTranslatorServiceEconomical"/> class.
    /// </summary>
    /// <param name="chatGptTranslatorService">The GPT translation service to use.</param>
    /// <param name="sourceLanguage">The source language code.</param>
    /// <param name="targetLanguage">The target language code.</param>
    /// <param name="maxTokens">The maximum number of tokens allowed. (Optional)</param>
    /// <param name="model">The model to use for translation. (Optional)</param>
    /// <param name="temperature">The creative temperature. (Optional)</param>
    /// <param name="user">The user ID. (Optional)</param>
    /// <param name="sendRequestAfterInactivity">The timespan for sending requests after inactivity. (Optional)</param>
    /// <param name="maxTokensPerRequest">The maximum tokens per request. (Optional)</param>
    public ChatGPTTranslatorServiceEconomical(
        IChatGPTTranslatorService chatGptTranslatorService,
        string sourceLanguage,
        string targetLanguage,
        int? maxTokens = null,
        string? model = null,
        float temperature = ChatCompletionTemperatures.Default,
        string? user = null,
        TimeSpan? sendRequestAfterInactivity = null,
        int? maxTokensPerRequest = null
    )
    {
        _chatGptTranslatorService = chatGptTranslatorService ??
                                    throw new ArgumentNullException(nameof(chatGptTranslatorService));
        _sourceLanguage = sourceLanguage ?? throw new ArgumentNullException(nameof(sourceLanguage));
        _targetLanguage = targetLanguage ?? throw new ArgumentNullException(nameof(targetLanguage));
        _maxTokens = maxTokens;
        _model = ChatCompletionModels.FromString(model ?? ChatCompletionModels.Default);
        _temperature = temperature;
        _user = user;
        _sendRequestAfterInactivity = sendRequestAfterInactivity ?? DefaultSendRequestAfterInactivity;
        _maxTokensPerRequest = maxTokensPerRequest ?? GetMaxTokensPerRequestForModel(_model);
    }

    private static int GetMaxTokensPerRequestForModel(string model)
    {
        var limit = ChatCompletionModels.GetMaxTokensLimitForModel(model);
        return limit / 4;
    }

    public async ValueTask DisposeAsync()
    {
        lock (_syncLock)
        {
            if (_batch.Version > 0)
            {
                if (CanBeRan(_tcs))
                {
                    _ = SendRequestAndResetBatch(_batch, _tcs);
                }
            }
        }
        if (_tcs.Task is { IsCompleted: false })
        {
            await _tcs.Task;
        }
    }

    /// <summary>
    /// Translates the given text.
    /// </summary>
    /// <param name="text">The text to translate.</param>
    /// <returns>A task representing the translated text.</returns>
    public async Task<string> TranslateText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        int index;
        TaskCompletionSource<Batch> tcs;
        lock (_syncLock)
        {
            var batch = GetRelevantBatch();
            tcs = _tcs;
            index = batch.Add(text);
            _ = RunRequestSendingInactivityTimer(tcs, batch);
        }

        var result = await tcs.Task;
        return result.Texts[index];
    }

    // Debouncer
    private async Task RunRequestSendingInactivityTimer(TaskCompletionSource<Batch> tcs, Batch batch)
    {
        await Task.Delay(_sendRequestAfterInactivity);
        lock (_syncLock)
        {
            var isBatchRelevant = ReferenceEquals(_batch, batch) && batch.Version == _batch.Version;
            if (isBatchRelevant && CanBeRan(tcs))
            {
                _ = SendRequestAndResetBatch(batch, tcs);
            }
        }
    }

    private Batch GetRelevantBatch()
    {
        var tcs = _tcs;
        if (_batch.GetTotalLength() > _maxTokensPerRequest)
        {
            _ = SendRequestAndResetBatch(_batch, tcs);
        }

        return _batch;
    }

    private async Task SendRequestAndResetBatch(Batch batch, TaskCompletionSource<Batch> tcs)
    {
        if (!CanBeRan(tcs))
        {
            throw new InvalidOperationException("Task is already completed or ran.");
        }

        lock (_syncLock)
        {
            _tcs = new();
            _batch = new();
        }

        try
        {
            var response = await _chatGptTranslatorService.TranslateObject(
                batch, true, _sourceLanguage, _targetLanguage, _maxTokens, _model, _temperature, _user);

            tcs.SetResult(response);
        }
        catch (Exception e)
        {
            tcs.SetException(e);
        }
    }

    private static bool CanBeRan(TaskCompletionSource<Batch> tcs)
    {
        return tcs.Task.Status is TaskStatus.Created or TaskStatus.WaitingForActivation;
    }

    internal class Batch
    {
        public IReadOnlyDictionary<int, string> Texts => _texts;
        
        [JsonIgnore]
        public int Version => _indexer;

        private readonly Dictionary<int, string> _texts;
        private int _indexer;

        [JsonConstructor]
        public Batch(IReadOnlyDictionary<int, string> texts)
        {
            _texts = new Dictionary<int, string>(texts) ?? throw new ArgumentNullException(nameof(texts));
        }

        public Batch()
        {
            _texts = new();
        }

        public int Add(string text)
        {
            _indexer++;
            _texts.Add(_indexer, text);
            return _indexer;
        }

        public int GetTotalLength()
        {
            return _texts.Values.Sum(it => it.Length);
        }
    }
}