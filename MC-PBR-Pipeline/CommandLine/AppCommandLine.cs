using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace McPbrPipeline.CommandLine
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

            //root.AddCommand(new ConvertCommand(provider).Command);
            root.AddCommand(new ImportCommand(provider).Command);
            root.AddCommand(new GenerateCommand(provider).Command);
            root.AddCommand(new PublishCommand(provider).Command);
        }


        public Task<int> RunAsync(string[] arguments) => root.InvokeAsync(arguments);
    }
}
