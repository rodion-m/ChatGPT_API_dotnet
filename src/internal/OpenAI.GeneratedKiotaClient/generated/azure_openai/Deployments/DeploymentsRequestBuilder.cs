// <auto-generated/>
using Microsoft.Kiota.Abstractions;
using OpenAI.Azure.GeneratedKiotaClient.Deployments.Item;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace OpenAI.Azure.GeneratedKiotaClient.Deployments {
    /// <summary>
    /// Builds and executes requests for operations under \deployments
    /// </summary>
    internal class DeploymentsRequestBuilder : BaseRequestBuilder {
        /// <summary>Gets an item from the OpenAI.Azure.GeneratedKiotaClient.deployments.item collection</summary>
        /// <param name="position">Unique identifier of the item</param>
        public DeploymentItemRequestBuilder this[string position] { get {
            var urlTplParams = new Dictionary<string, object>(PathParameters);
            urlTplParams.Add("deployment%2Did", position);
            return new DeploymentItemRequestBuilder(urlTplParams, RequestAdapter);
        } }
        /// <summary>
        /// Instantiates a new DeploymentsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="pathParameters">Path parameters for the request</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public DeploymentsRequestBuilder(Dictionary<string, object> pathParameters, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments", pathParameters) {
        }
        /// <summary>
        /// Instantiates a new DeploymentsRequestBuilder and sets the default values.
        /// </summary>
        /// <param name="rawUrl">The raw URL to use for the request builder.</param>
        /// <param name="requestAdapter">The request adapter to use to execute the requests.</param>
        public DeploymentsRequestBuilder(string rawUrl, IRequestAdapter requestAdapter) : base(requestAdapter, "{+baseurl}/deployments", rawUrl) {
        }
    }
}

