// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using OpenAI.Azure.GeneratedKiotaClient.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
namespace OpenAI.Azure.GeneratedKiotaClient.Deployments.Item.Audio.Translations {
    /// <summary>
    /// Builds and executes requests for operations under \deployments\{deployment-id}\audio\translations
    /// </summary>
    internal class TranslationsRequestBuilder : BaseRequestBuilder {
        /// <summary>
        /// Instantiates a new TranslationsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public TranslationsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments/{deployment%2Did}/audio/translations{?api%2Dversion*}", pathParameters) {
        }
        /// <summary>
        /// Instantiates a new TranslationsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public TranslationsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments/{deployment%2Did}/audio/translations{?api%2Dversion*}", rawUrl) {
        }
        /// <summary>
        /// Transcribes and translates input audio into English text.
        /// </summary>
        /// <param name="body">Translation request.</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<TranslationsPostResponse?> PostAsTranslationsPostResponseAsync(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration>? requestConfiguration = default, CancellationToken cancellationToken = default) {
#nullable restore
#else
        public async Task<TranslationsPostResponse> PostAsTranslationsPostResponseAsync(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration> requestConfiguration = default, CancellationToken cancellationToken = default) {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPostRequestInformation(body, requestConfiguration);
            return await RequestAdapter.SendAsync<TranslationsPostResponse>(requestInfo, TranslationsPostResponse.CreateFromDiscriminatorValue, default, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Transcribes and translates input audio into English text.
        /// </summary>
        /// <param name="body">Translation request.</param>
        /// <param name="cancellationToken">Cancellation token to use when cancelling requests</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
        [Obsolete("This method is obsolete. Use PostAsTranslationsPostResponse instead.")]
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public async Task<TranslationsResponse?> PostAsync(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration>? requestConfiguration = default, CancellationToken cancellationToken = default) {
#nullable restore
#else
        public async Task<TranslationsResponse> PostAsync(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration> requestConfiguration = default, CancellationToken cancellationToken = default) {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = ToPostRequestInformation(body, requestConfiguration);
            return await RequestAdapter.SendAsync<TranslationsResponse>(requestInfo, TranslationsResponse.CreateFromDiscriminatorValue, default, cancellationToken).ConfigureAwait(false);
        }
        /// <summary>
        /// Transcribes and translates input audio into English text.
        /// </summary>
        /// <param name="body">Translation request.</param>
        /// <param name="requestConfiguration">Configuration for the request such as headers, query parameters, and middleware options.</param>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RequestInformation ToPostRequestInformation(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration>? requestConfiguration = default) {
#nullable restore
#else
        public RequestInformation ToPostRequestInformation(MultipartBody body, Action<TranslationsRequestBuilderPostRequestConfiguration> requestConfiguration = default) {
#endif
            _ = body ?? throw new ArgumentNullException(nameof(body));
            var requestInfo = new RequestInformation {
                HttpMethod = Method.POST,
                UrlTemplate = UrlTemplate,
                PathParameters = PathParameters,
            };
            if (requestConfiguration != null) {
                var requestConfig = new TranslationsRequestBuilderPostRequestConfiguration();
                requestConfiguration.Invoke(requestConfig);
                requestInfo.AddQueryParameters(requestConfig.QueryParameters);
                requestInfo.AddRequestOptions(requestConfig.Options);
                requestInfo.AddHeaders(requestConfig.Headers);
            }
            requestInfo.Headers.TryAdd("Accept", "application/json");
            requestInfo.SetContentFromParsable(RequestAdapter, "multipart/form-data", body);
            return requestInfo;
        }
        /// <summary>
        /// Returns a request builder with the provided arbitrary URL. Using this method means any other path or query parameters are ignored.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        public TranslationsRequestBuilder WithUrl(string rawUrl) {
            return new TranslationsRequestBuilder(rawUrl, RequestAdapter);
        }
        /// <summary>
        /// Composed type wrapper for classes audioResponse, audioVerboseResponse
        /// </summary>
        internal class TranslationsPostResponse : IComposedTypeWrapper, IParsable {
            /// <summary>Composed type representation for type audioResponse</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse? AudioResponse { get; set; }
#nullable restore
#else
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse AudioResponse { get; set; }
#endif
            /// <summary>Composed type representation for type audioVerboseResponse</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse? AudioVerboseResponse { get; set; }
#nullable restore
#else
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse AudioVerboseResponse { get; set; }
#endif
            /// <summary>
            /// Creates a new instance of the appropriate class based on discriminator value
            /// </summary>
            /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
            public static TranslationsPostResponse CreateFromDiscriminatorValue(IParseNode parseNode) {
                _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
                var mappingValue = parseNode.GetChildNode("")?.GetStringValue();
                var result = new TranslationsPostResponse();
                if("audioResponse".Equals(mappingValue, StringComparison.OrdinalIgnoreCase)) {
                    result.AudioResponse = new OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse();
                }
                else if("audioVerboseResponse".Equals(mappingValue, StringComparison.OrdinalIgnoreCase)) {
                    result.AudioVerboseResponse = new OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse();
                }
                return result;
            }
            /// <summary>
            /// The deserialization information for the current model
            /// </summary>
            public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
                if(AudioResponse != null) {
                    return AudioResponse.GetFieldDeserializers();
                }
                else if(AudioVerboseResponse != null) {
                    return AudioVerboseResponse.GetFieldDeserializers();
                }
                return new Dictionary<string, Action<IParseNode>>();
            }
            /// <summary>
            /// Serializes information the current object
            /// </summary>
            /// <param name="writer">Serialization writer to use to serialize this model</param>
            public virtual void Serialize(ISerializationWriter writer) {
                _ = writer ?? throw new ArgumentNullException(nameof(writer));
                if(AudioResponse != null) {
                    writer.WriteObjectValue<OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse>(null, AudioResponse);
                }
                else if(AudioVerboseResponse != null) {
                    writer.WriteObjectValue<OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse>(null, AudioVerboseResponse);
                }
            }
        }
        /// <summary>
        /// Transcribes and translates input audio into English text.
        /// </summary>
        internal class TranslationsRequestBuilderPostQueryParameters {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            [QueryParameter("api%2Dversion")]
            public string? ApiVersion { get; set; }
#nullable restore
#else
            [QueryParameter("api%2Dversion")]
            public string ApiVersion { get; set; }
#endif
        }
        /// <summary>
        /// Configuration for the request such as headers, query parameters, and middleware options.
        /// </summary>
        internal class TranslationsRequestBuilderPostRequestConfiguration {
            /// <summary>Request headers</summary>
            public RequestHeaders Headers { get; set; }
            /// <summary>Request options</summary>
            public IList<IRequestOption> Options { get; set; }
            /// <summary>Request query parameters</summary>
            public TranslationsRequestBuilderPostQueryParameters QueryParameters { get; set; } = new TranslationsRequestBuilderPostQueryParameters();
            /// <summary>
            /// Instantiates a new translationsRequestBuilderPostRequestConfiguration and sets the default values.
            /// </summary>
            public TranslationsRequestBuilderPostRequestConfiguration() {
                Options = new List<IRequestOption>();
                Headers = new RequestHeaders();
            }
        }
        /// <summary>
        /// Composed type wrapper for classes audioResponse, audioVerboseResponse
        /// </summary>
        internal class TranslationsResponse : IComposedTypeWrapper, IParsable {
            /// <summary>Composed type representation for type audioResponse</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse? AudioResponse { get; set; }
#nullable restore
#else
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse AudioResponse { get; set; }
#endif
            /// <summary>Composed type representation for type audioVerboseResponse</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse? AudioVerboseResponse { get; set; }
#nullable restore
#else
            public OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse AudioVerboseResponse { get; set; }
#endif
            /// <summary>
            /// Creates a new instance of the appropriate class based on discriminator value
            /// </summary>
            /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
            public static TranslationsResponse CreateFromDiscriminatorValue(IParseNode parseNode) {
                _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
                var mappingValue = parseNode.GetChildNode("")?.GetStringValue();
                var result = new TranslationsResponse();
                if("audioResponse".Equals(mappingValue, StringComparison.OrdinalIgnoreCase)) {
                    result.AudioResponse = new OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse();
                }
                else if("audioVerboseResponse".Equals(mappingValue, StringComparison.OrdinalIgnoreCase)) {
                    result.AudioVerboseResponse = new OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse();
                }
                return result;
            }
            /// <summary>
            /// The deserialization information for the current model
            /// </summary>
            public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
                if(AudioResponse != null) {
                    return AudioResponse.GetFieldDeserializers();
                }
                else if(AudioVerboseResponse != null) {
                    return AudioVerboseResponse.GetFieldDeserializers();
                }
                return new Dictionary<string, Action<IParseNode>>();
            }
            /// <summary>
            /// Serializes information the current object
            /// </summary>
            /// <param name="writer">Serialization writer to use to serialize this model</param>
            public virtual void Serialize(ISerializationWriter writer) {
                _ = writer ?? throw new ArgumentNullException(nameof(writer));
                if(AudioResponse != null) {
                    writer.WriteObjectValue<OpenAI.Azure.GeneratedKiotaClient.Models.AudioResponse>(null, AudioResponse);
                }
                else if(AudioVerboseResponse != null) {
                    writer.WriteObjectValue<OpenAI.Azure.GeneratedKiotaClient.Models.AudioVerboseResponse>(null, AudioVerboseResponse);
                }
            }
        }
    }
}
