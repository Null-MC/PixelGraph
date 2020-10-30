using McPbrPipeline.Internal;
using McPbrPipeline.Internal.Extensions;
using McPbrPipeline.Internal.Input;
using McPbrPipeline.Internal.Output;
using McPbrPipeline.Internal.Textures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace McPbrPipeline.CommandLine
{
    internal class ConvertCommand
    {
        private readonly ProviderFactory factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public ConvertCommand(
            ProviderFactory factory,
            IAppLifetime lifetime,
            ILogger<ConvertCommand> logger)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("convert", "Converts a specified texture from a source format to a destination format.") {
                Handler = CommandHandler.Create<string, DirectoryInfo, string, string>(RunAsync),
            };

            Command.AddOption(new Option<string>(
                new [] {"-t", "--texture"},
                "The name of the texture to convert, excluding extension."));

            Command.AddOption(new Option<DirectoryInfo>(
                new[] { "-d", "--destination" },
                "The target directory to write the converted texture to."));

            Command.AddOption(new Option<string>(
                new[] { "-i", "--input-format" },
                "The format of the source texture."));

            Command.AddOption(new Option<string>(
                new[] { "-o", "--output-format" },
                "The target format of the converted texture."));
        }

        private async Task<int> RunAsync(string texture, DirectoryInfo destination, string inputFormat, string outputFormat)
        {
            if (texture == null) {
                ConsoleEx.WriteLine("Source texture is undefined!", ConsoleColor.DarkRed);
                return -1;
            }

            if (destination == null) {
                ConsoleEx.WriteLine("Destination path is undefined!", ConsoleColor.DarkRed);
                return -1;
            }

            ConsoleEx.WriteLine("\nConverting...", ConsoleColor.White);
            ConsoleEx.Write("  Texture     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(texture, ConsoleColor.Cyan);
            ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(destination.FullName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Format-In   : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(inputFormat, ConsoleColor.Cyan);
            ConsoleEx.Write("  Format-Out  : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(outputFormat, ConsoleColor.Cyan);
            ConsoleEx.WriteLine();

            await using var commandProvider = factory.Build(false);
            var reader = commandProvider.GetRequiredService<IInputReader>();
            var writer = commandProvider.GetRequiredService<IOutputWriter>();

            var fullFile = Path.GetFullPath(texture);

            var packReader = new PackReader();
            var pack = await packReader.ReadAsync(fullFile, token: lifetime.Token);
            pack.Properties["input.format"] = inputFormat;
            pack.Properties["output.format"] = outputFormat;

            var pbrTexture = new PbrProperties {
                UseGlobalMatching = true,
                Name = Path.GetFileName(texture),
                Path = ".",
            };

            reader.SetRoot(pack.Source);
            writer.SetRoot(destination.FullName);

            var timer = Stopwatch.StartNew();

            try {
                var graph = commandProvider.GetRequiredService<ITextureGraphBuilder>();
                await graph.BuildAsync(pack, pbrTexture, lifetime.Token);
                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while converting!");
                return -1;
            }
            finally {
                timer.Stop();

                ConsoleEx.Write("\nDuration: ", ConsoleColor.Gray);
                ConsoleEx.WriteLine($"{timer.Elapsed:g}", ConsoleColor.Cyan);
            }
        }
    }
}
