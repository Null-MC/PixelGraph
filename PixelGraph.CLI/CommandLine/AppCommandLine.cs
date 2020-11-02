using System.CommandLine;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal interface IAppCommandLine
    {
        Task<int> RunAsync(string[] arguments);
    }

    internal class AppCommandLine : IAppCommandLine
    {
        private readonly RootCommand root;


        public AppCommandLine(
            ImportCommand importCommand,
            GenerateCommand generateCommand,
            PublishCommand publishCommand)
        {
            root = new RootCommand();

            //root.AddCommand(new ConvertCommand(provider).Command);
            root.AddCommand(importCommand.Command);
            root.AddCommand(generateCommand.Command);
            root.AddCommand(publishCommand.Command);
        }


        public Task<int> RunAsync(string[] arguments) => root.InvokeAsync(arguments);
    }
}
