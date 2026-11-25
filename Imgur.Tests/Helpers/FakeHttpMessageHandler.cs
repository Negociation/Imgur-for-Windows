using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Imgur.Tests.Helpers
{
    /// <summary>
    /// Fake handler for API TDD — queues canned responses without hitting the network.
    /// </summary>
    public sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly System.Collections.Generic.Queue<HttpResponseMessage> _responses =
            new System.Collections.Generic.Queue<HttpResponseMessage>();

        public System.Collections.Generic.List<HttpRequestMessage> Requests { get; } =
            new System.Collections.Generic.List<HttpRequestMessage>();

        public void Enqueue(HttpResponseMessage response)
        {
            _responses.Enqueue(response);
        }

        public void EnqueueJson(HttpStatusCode status, string json)
        {
            Enqueue(new HttpResponseMessage(status)
            {
                Content = new StringContent(json ?? string.Empty)
            });
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            if (_responses.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("No queued response")
                });
            }
            return Task.FromResult(_responses.Dequeue());
        }
    }
}
