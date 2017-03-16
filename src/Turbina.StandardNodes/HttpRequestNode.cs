using System;
using System.Net.Http;
using System.Threading.Tasks;
using Turbina.Engine;

namespace Turbina.StandardNodes
{
    public interface IHttpRequestNode : INode
    {
        IObserver<Uri> Uri { get; }

        IObservable<string> Content { get; }
    }

    internal sealed class HttpRequestNode : Node, IHttpRequestNode
    {
        public Inlet<Uri> Uri { get; set; }

        public Outlet<string> Content { get; set; }

        public HttpRequestNode(Workspace workspace) : base(workspace)
        {
        }

        IObserver<Uri> IHttpRequestNode.Uri => Uri;

        IObservable<string> IHttpRequestNode.Content => Content;

        protected override async Task Operate()
        {
            Content.Send(await new HttpClient().GetStringAsync(await Uri));
        }
    }
}