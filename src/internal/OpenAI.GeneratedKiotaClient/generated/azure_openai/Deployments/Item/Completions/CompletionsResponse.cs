// <auto-generated/>
using Microsoft.Kiota.Abstractions.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
namespace OpenAI.Azure.GeneratedKiotaClient.Deployments.Item.Completions {
    [Obsolete("This class is obsolete. Use completionsPostResponse instead.")]
    internal class CompletionsResponse : CompletionsPostResponse, IParsable {
        /// <summary>
        /// Creates a new instance of the appropriate class based on discriminator value
        /// </summary>
        /// <param name="parseNode">The parse node to use to read the discriminator value and create the object</param>
        public static new CompletionsResponse CreateFromDiscriminatorValue(IParseNode parseNode) {
            _ = parseNode ?? throw new ArgumentNullException(nameof(parseNode));
            return new CompletionsResponse();
        }
    }
}
