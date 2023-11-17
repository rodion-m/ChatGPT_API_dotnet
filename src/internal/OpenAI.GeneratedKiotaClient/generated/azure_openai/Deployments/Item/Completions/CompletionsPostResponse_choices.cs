// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using OpenAI.Azure.GeneratedKiotaClient.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenAI.Azure.GeneratedKiotaClient.Deployments.Item.Completions {
    internal class CompletionsPostResponse_choices : IAdditionalDataHolder, IParsable {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>Information about the content filtering category (hate, sexual, violence, self_harm), if it has been detected, as well as the severity level (very_low, low, medium, high-scale that determines the intensity and risk level of harmful content) and if it has been filtered or not. Information about third party text and profanity, if it has been detected, and if it has been filtered or not. And information about customer block list, if it has been filtered and its id.</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public ContentFilterChoiceResults? ContentFilterResults { get; set; }
#nullable restore
#else
        public ContentFilterChoiceResults ContentFilterResults { get; set; }
#endif
        /// <summary>The finish_reason property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? FinishReason { get; set; }
#nullable restore
#else
        public string FinishReason { get; set; }
#endif
        /// <summary>The index property</summary>
        public int? Index { get; set; }
        /// <summary>The logprobs property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public CompletionsPostResponse_choices_logprobs? Logprobs { get; set; }
#nullable restore
#else
        public CompletionsPostResponse_choices_logprobs Logprobs { get; set; }
#endif
        /// <summary>The text property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public string? Text { get; set; }
#nullable restore
#else
        public string Text { get; set; }
#endif
        /// <summary>
        /// Instantiates a new completionsPostResponse_choices and sets the default values.
        /// </summary>
        public CompletionsPostResponse_choices() {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static CompletionsPostResponse_choices CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new CompletionsPostResponse_choices();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"content_filter_results", n => { ContentFilterResults = n.GetObjectValue<ContentFilterChoiceResults>(ContentFilterChoiceResults.CreateFromDiscriminatorValue); } },
                {"finish_reason", n => { FinishReason = n.GetStringValue(); } },
                {"index", n => { Index = n.GetIntValue(); } },
                {"logprobs", n => { Logprobs = n.GetObjectValue<CompletionsPostResponse_choices_logprobs>(CompletionsPostResponse_choices_logprobs.CreateFromDiscriminatorValue); } },
                {"text", n => { Text = n.GetStringValue(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<ContentFilterChoiceResults>("content_filter_results", ContentFilterResults);
            writer.WriteStringValue("finish_reason", FinishReason);
            writer.WriteIntValue("index", Index);
            writer.WriteObjectValue<CompletionsPostResponse_choices_logprobs>("logprobs", Logprobs);
            writer.WriteStringValue("text", Text);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}

