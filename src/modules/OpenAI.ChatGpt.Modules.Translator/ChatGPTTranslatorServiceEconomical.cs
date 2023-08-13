using System.Text.Json.Serialization;
using OpenAI.ChatGpt.Models.ChatCompletion;

namespace OpenAI.ChatGpt.Modules.Translator;

public class ChatGPTTranslatorServiceEconomical : IAsyncDisposable
{
    private readonly ChatGPTTranslatorService _chatGptTranslatorService;
    private readonly string _sourceLanguage;
    private readonly string _targetLanguage;
    private readonly int? _maxTokens;
    private readonly string _model;
    private readonly float _temperature;
    private readonly string? _user;
    private readonly int? _maxTokensPerRequest;
    private readonly TimeSpan _sendRequestAfterInactivity;
    private static readonly TimeSpan DefaultSendRequestAfterInactivity = TimeSpan.FromMilliseconds(100);
    
    private Buffer _buffer = new();
    private TaskCompletionSource<Buffer> _tcs = new();
    private readonly object _syncLock = new();

    public ChatGPTTranslatorServiceEconomical(
        ChatGPTTranslatorService chatGptTranslatorService,
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
            if (_buffer.Version > 0)
            {
                if (CanBeRan(_tcs))
                {
                    _ = SendRequestAndResetBuffer(_buffer, _tcs);
                }
            }
        }
        if (_tcs.Task is { IsCompleted: false })
        {
            await _tcs.Task;
        }
    }

    public async Task<string> TranslateText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        int index;

        TaskCompletionSource<Buffer> tcs;
        lock (_syncLock)
        {
            var buffer = GetBuffer();
            tcs = _tcs;
            index = buffer.Add(text);
            _ = RunRequestSendingInactivityTimer(tcs, buffer);
        }

        var result = await tcs.Task;
        return result.Texts[index];
    }

    // Debouncer
    private async Task RunRequestSendingInactivityTimer(TaskCompletionSource<Buffer> tcs, Buffer buffer)
    {
        await Task.Delay(_sendRequestAfterInactivity);
        lock (_syncLock)
        {
            var isBufferRelevant = ReferenceEquals(_buffer, buffer) && buffer.Version == _buffer.Version;
            if (isBufferRelevant && CanBeRan(tcs))
            {
                _ = SendRequestAndResetBuffer(buffer, tcs);
            }
        }
    }

    private Buffer GetBuffer()
    {
        var tcs = _tcs;
        if (_buffer.GetTotalLength() > _maxTokensPerRequest)
        {
            _ = SendRequestAndResetBuffer(_buffer, tcs);
        }

        return _buffer;
    }

    private async Task SendRequestAndResetBuffer(Buffer buffer, TaskCompletionSource<Buffer> tcs)
    {
        if (!CanBeRan(tcs))
        {
            throw new InvalidOperationException("Task is already completed or ran.");
        }

        lock (_syncLock)
        {
            _tcs = new();
            _buffer = new();
        }

        try
        {
            var response = await _chatGptTranslatorService.TranslateObject(
                buffer, true, _sourceLanguage, _targetLanguage, _maxTokens, _model, _temperature, _user);

            tcs.SetResult(response);
        }
        catch (Exception e)
        {
            tcs.SetException(e);
        }
    }

    private static bool CanBeRan(TaskCompletionSource<Buffer> tcs)
    {
        return tcs.Task.Status is TaskStatus.Created or TaskStatus.WaitingForActivation;
    }

    private class Buffer
    {
        public IReadOnlyDictionary<int, string> Texts => _texts;
        
        [JsonIgnore]
        public int Version => _indexer;

        private readonly Dictionary<int, string> _texts;
        private int _indexer;

        [JsonConstructor]
        public Buffer(IReadOnlyDictionary<int, string> texts)
        {
            _texts = new Dictionary<int, string>(texts) ?? throw new ArgumentNullException(nameof(texts));
        }

        public Buffer()
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