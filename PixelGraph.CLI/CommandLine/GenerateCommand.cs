using System.CommandLine;

namespace PixelGraph.CLI.CommandLine
{
    internal class GenerateCommand
    {
        public Command Command {get;}


        public GenerateCommand(
            GenerateNormalCommand generateNormalCommand,
            GenerateOcclusionCommand generateOcclusionCommand)
        {
            Command = new Command("generate", "Generate textures.");
            Command.AddCommand(generateNormalCommand.Command);
            Command.AddCommand(generateOcclusionCommand.Command);
        }
    }
}
