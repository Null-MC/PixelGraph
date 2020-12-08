using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Textures;
using SixLabors.ImageSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine
{
    internal class GenerateNormalCommand
    {
        private readonly IServiceBuilder factory;
        private readonly IAppLifetime lifetime;
        private readonly ILogger logger;

        public Command Command {get;}


        public GenerateNormalCommand(
            ILogger<GenerateNormalCommand> logger,
            IServiceBuilder factory,
            IAppLifetime lifetime)
        {
            this.factory = factory;
            this.lifetime = lifetime;
            this.logger = logger;

            Command = new Command("normal", "Generates a normal texture from a specified height texture.") {
                Handler = CommandHandler.Create<FileInfo, FileInfo, string, string[]>(RunAsync),
            };

            Command.AddOption(new Option<FileInfo>(
                new [] {"--pbr"},
                () => new FileInfo("pbr.properties"),
                "The optional name of a PBR properties file containing settings for normal-texture generation. Defaults to 'pbr.properties'."));

            Command.AddOption(new Option<FileInfo>(
                new [] {"-h", "--height"},
                "The name of the height texture to use for generating normals. Defaults to 'height.*'."));

            Command.AddOption(new Option<string>(
                new [] {"-n", "--normal"},
                () => "normal.png",
                "The name of the normal texture to generate. Defaults to 'normal.png'."));

            Command.AddOption(new Option<string[]>(
                new[] {"--property" },
                "Override a pack property."));
        }

        private async Task<int> RunAsync(FileInfo pbr, FileInfo height, string normal, string[] property)
        {
            factory.AddFileInput();
            factory.AddFileOutput();

            factory.Services.AddTransient<Executor>();
            await using var provider = factory.Build();

            try {
                var executor = provider.GetRequiredService<Executor>();
                await executor.ExecuteAsync(pbr.FullName, height.FullName, normal, property, lifetime.Token);
                return 0;
            }
            catch (ApplicationException error) {
                ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
                ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
                return -1;
            }
            catch (Exception error) {
                logger.LogError(error, "An unhandled exception occurred while generating normal texture!");
                return -1;
            }
        }

        internal class Executor
        {
            private readonly IServiceProvider provider;
            private readonly INamingStructure naming;
            private readonly IInputReader reader;
            private readonly IResourcePackReader packReader;
            private readonly IMaterialReader materialReader;
            private readonly ILogger logger;


            public Executor(
                IServiceProvider provider,
                INamingStructure naming,
                IInputReader reader,
                IResourcePackReader packReader,
                IMaterialReader materialReader,
                ILogger<Executor> logger)
            {
                this.provider = provider;
                this.naming = naming;
                this.reader = reader;
                this.packReader = packReader;
                this.materialReader = materialReader;
                this.logger = logger;
            }

            public async Task ExecuteAsync(string pbrFilename, string heightFilename, string normalFilename, string[] properties, CancellationToken token = default)
            {
                var root = Path.GetDirectoryName(pbrFilename) ?? ".";
                var packName = Path.GetFileName(pbrFilename);
                var inputName = PathEx.Join(root, "input.yml");

                reader.SetRoot(root);

                var packInput = await packReader.ReadInputAsync(inputName);
                var packProfile = await packReader.ReadProfileAsync(pbrFilename);

                if (properties != null) {
                    // TODO: apply properties?
                }

                var material = await materialReader.LoadAsync(packName, token);

                if (heightFilename != null && File.Exists(heightFilename)) {
                    var heightName = Path.GetFileName(heightFilename);

                    material.Name = heightName;
                    material.Height.Texture = heightFilename;
                }

                var timer = Stopwatch.StartNew();

                logger.LogDebug("Generating normals for texture {DisplayName}.", material.DisplayName);
                var finalName = normalFilename ?? naming.GetOutputTextureName(packProfile, material.Name, TextureTags.Normal, material.UseGlobalMatching);

                try {
                    var context = new MaterialContext {
                        Input = packInput,
                        Profile = packProfile,
                        Material = material,
                    };

                    using var graph = provider.GetRequiredService<ITextureGraph>();
                    graph.InputEncoding.AddRange(context.Input.GetMapped());
                    graph.OutputEncoding.AddRange(context.Input.GetMapped());
                    graph.Context = context;

                    using var image = await graph.GenerateNormalAsync(token);

                    await image.SaveAsync(finalName, token);
                    logger.LogInformation("Normal texture {finalName} generated successfully.", finalName);
                }
                catch (SourceEmptyException error) {
                    logger.LogError($"Failed to generate Normal texture {{finalName}}! {error.Message}", finalName);
                }
                catch (Exception error) {
                    logger.LogError(error, "Failed to generate Normal texture {finalName}!", finalName);
                }

                timer.Stop();
                var duration = timer.Elapsed.ToString("g");
                logger.LogDebug("Duration: {duration}", duration);
            }
        }
    }
}
