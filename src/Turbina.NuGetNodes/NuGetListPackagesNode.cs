using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Turbina.Engine;
using Task = System.Threading.Tasks.Task;

namespace Turbina.NuGetNodes
{
    public class NuGetListPackagesNode : Node
    {
        public NuGetListPackagesNode(Workspace workspace) : base(workspace)
        {
            Settings = Inlets.Create<ISettings>(nameof(Settings));
            Repo = Outlets.Create<string>(nameof(Repo));

            Settings.OnNext(NuGet.Configuration.Settings.LoadDefaultSettings("."));
        }

        public Inlet<ISettings> Settings { get; }
        public Outlet<string> Repo { get; }

        protected override async Task Operate()
        {
//            var resourceProviders = new[] { new Lazy<INuGetResourceProvider>(() => new AutoCompleteResourceV3Provider()) };
//            var sourceRepositoryProvider = new SourceRepositoryProvider(await Settings, resourceProviders);
//            var sourceRepositories = sourceRepositoryProvider.GetRepositories();
//            foreach (var sourceRepository in sourceRepositories)
//            {
//                var tuple = await new AutoCompleteResourceV3Provider().TryCreate(sourceRepository, NodeDisposedToken);
//                if (tuple.Item1)
//                {
//                    var nuGetResource = (AutoCompleteResourceV3) tuple.Item2;
//                    var enumerableAsync = await nuGetResource.IdStartsWith("nuget", false, NullLogger.Instance, NodeDisposedToken);
//                    foreach (var VARIABLE in enumerableAsync)
//                    {
//                        Repo.Send(VARIABLE);
//                        await Task.Delay(1000);
//                    }
//                    /*var enumeratorAsync = enumerableAsync.GetEnumeratorAsync();
//                    while (await enumeratorAsync.MoveNextAsync())
//                    {
//                        var current = enumeratorAsync.Current;
//                        Repo.Send(current.Identity.ToString());
//                        await Task.Delay(1000);
//                    }*/
//                }
//            }
        }
    }
}
