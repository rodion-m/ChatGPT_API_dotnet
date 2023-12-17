// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenAI.GeneratedKiotaClient.Models {
    internal class RunStepDetailsToolCallsCodeOutputImageObject : IAdditionalDataHolder, IParsable {
        /// <summary>Stores additional data not described in the OpenAPI description found when deserializing. Can be used for serialization as well.</summary>
        public IDictionary<string, object> AdditionalData { get; set; }
        /// <summary>The image property</summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_1_OR_GREATER
#nullable enable
        public RunStepDetailsToolCallsCodeOutputImageObject_image? Image { get; set; }
#nullable restore
#else
        public RunStepDetailsToolCallsCodeOutputImageObject_image Image { get; set; }
#endif
        /// <summary>Always `image`.</summary>
        public RunStepDetailsToolCallsCodeOutputImageObject_type? Type { get; set; }
        /// <summary>
        /// Instantiates a new RunStepDetailsToolCallsCodeOutputImageObject and sets the default values.
        /// </summary>
        public RunStepDetailsToolCallsCodeOutputImageObject() {
            AdditionalData = new Dictionary<string, object>();
        }
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static RunStepDetailsToolCallsCodeOutputImageObject CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new RunStepDetailsToolCallsCodeOutputImageObject();
        }
        /// <summary>
        /// The deserialization information for the current model
        /// </summary>
        public virtual IDictionary<string, Action<IParseNode>> GetFieldDeserializers() {
            return new Dictionary<string, Action<IParseNode>> {
                {"image", n => { Image = n.GetObjectValue<RunStepDetailsToolCallsCodeOutputImageObject_image>(RunStepDetailsToolCallsCodeOutputImageObject_image.CreateFromDiscriminatorValue); } },
                {"type", n => { Type = n.GetEnumValue<RunStepDetailsToolCallsCodeOutputImageObject_type>(); } },
            };
        }
        /// <summary>
        /// Serializes information the current object
        /// </summary>
        /// <param name="writer">Serialization writer to use to serialize this model</param>
        public virtual void Serialize(ISerializationWriter writer) {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));
            writer.WriteObjectValue<RunStepDetailsToolCallsCodeOutputImageObject_image>("image", Image);
            writer.WriteEnumValue<RunStepDetailsToolCallsCodeOutputImageObject_type>("type", Type);
            writer.WriteAdditionalData(AdditionalData);
        }
    }
}
