using McPbrPipeline.CommandLine;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace McPbrPipeline
{
    internal interface IAppCommandLine
    {
        Task<int> RunAsync(string[] arguments);
    }

    internal class AppCommandLine : IAppCommandLine
    {
        private readonly RootCommand root;


        public AppCommandLine(IServiceProvider provider)
        {
            root = new RootCommand();

            root.AddCommand(new PublishCommand(provider).Command);
        }


        public Task<int> RunAsync(string[] arguments) => root.InvokeAsync(arguments);
    }
}
