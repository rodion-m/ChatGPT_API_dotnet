// <auto-generated/>
using Microsoft.Kiota.Abstractions;
using OpenAI.Azure.GeneratedKiotaClient.Deployments.Item.Extensions.Chat.Completions;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace OpenAI.Azure.GeneratedKiotaClient.Deployments.Item.Extensions.Chat {
    /// <summary>
    /// Builds and executes requests for operations under \deployments\{deployment-id}\extensions\chat
    /// </summary>
    internal class ChatRequestBuilder : BaseRequestBuilder {
        /// <summary>The completions property</summary>
        public CompletionsRequestBuilder Completions { get =>
            new CompletionsRequestBuilder(PathParameters, RequestAdapter);
        }
        /// <summary>
        /// Instantiates a new ChatRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ChatRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments/{deployment%2Did}/extensions/chat", pathParameters) {
        }
        /// <summary>
        /// Instantiates a new ChatRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public ChatRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments/{deployment%2Did}/extensions/chat", rawUrl) {
        }
    }
}

