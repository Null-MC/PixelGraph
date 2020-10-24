using System;
using System.CommandLine;

namespace McPbrPipeline.CommandLine
{
    internal class GenerateCommand
    {
        public Command Command {get;}


        public GenerateCommand(IServiceProvider provider)
        {
            Command = new Command("generate", "Generate textures.");
            Command.AddCommand(new GenerateNormalCommand(provider).Command);
            Command.AddCommand(new GenerateOcclusionCommand(provider).Command);
        }
    }
}
