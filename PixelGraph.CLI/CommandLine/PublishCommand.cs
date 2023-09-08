using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelGraph.CLI.Extensions;
using PixelGraph.Common;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Publishing;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Projects;
using Serilog;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.CLI.CommandLine;

internal class PublishCommand
{
    private readonly ILogger<PublishCommand> logger;
    private readonly IServiceProvider provider;
    private readonly IAppLifetime lifetime;

    public Command Command {get;}


    public PublishCommand(
        ILogger<PublishCommand> logger,
        IServiceProvider provider,
        IAppLifetime lifetime)
    {
        this.provider = provider;
        this.lifetime = lifetime;
        this.logger = logger;

        Command = new Command("publish", "Publishes the specified profile.") {
            Handler = CommandHandler.Create<FileInfo, string, DirectoryInfo, FileInfo, bool, int>(RunAsync),
        };

        Command.AddOption(new Option<FileInfo>(
            new [] {"-p", "--project-file"}, () => new FileInfo("project.yml"),
            "The filename of the project to publish."));

        Command.AddOption(new Option<string>(
            new [] {"-n", "--profile-name"},
            "The name of the publish-profile within the project to publish."));

        Command.AddOption(new Option<DirectoryInfo>(
            new[] { "-o", "--output" },
            "The target directory to publish the resource pack to."));

        Command.AddOption(new Option<string>(
            new [] {"-z", "--zip"},
            "Generates a compressed ZIP archive of the published contents."));

        Command.AddOption(new Option<bool>(
            new [] {"-c", "--clean"}, () => false,
            "Generates a compressed ZIP archive of the published contents."));

        Command.AddOption(new Option<int>(
            new [] {"--concurrency"}, ConcurrencyHelper.GetDefaultValue,
            "Sets the level of concurrency for importing/publishing files. Default value is half of the system processor count."));
    }

    private async Task<int> RunAsync(FileInfo projectFile, string profileName, DirectoryInfo destination, FileInfo zip, bool clean, int concurrency)
    {
        var destPath = zip?.FullName ?? destination.FullName;

        try {
            if (projectFile?.Exists != true)
                throw new ApplicationException($"Project file not found! [{projectFile?.FullName}]");

            if (string.IsNullOrWhiteSpace(profileName))
                throw new ApplicationException("Profile name is undefined!");

            ProjectData projectData;
            await using (var stream = projectFile.OpenRead()) {
                projectData = ProjectSerializer.Parse(stream);
            }

            var profile = projectData.Profiles.Find(p => string.Equals(p.Name, profileName, StringComparison.InvariantCultureIgnoreCase));
            if (profile == null) throw new ApplicationException($"Profile not found in project! [{profileName}]");

            var context = new ProjectPublishContext {
                Project = projectData,
                Profile = profile,
                LastUpdated = projectFile.LastWriteTimeUtc,
            };

            ConsoleEx.WriteLine("\nPublishing...", ConsoleColor.White);
            ConsoleEx.Write("  Project     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(projectFile.FullName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Destination : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(destPath, ConsoleColor.Cyan);
            ConsoleEx.Write("  Profile     : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(profileName, ConsoleColor.Cyan);
            ConsoleEx.Write("  Concurrency : ", ConsoleColor.Gray);
            ConsoleEx.WriteLine(concurrency.ToString("N0"), ConsoleColor.Cyan);
            ConsoleEx.WriteLine();

            var executor = provider.GetRequiredService<Executor>();

            executor.Context = context;
            executor.Concurrency = concurrency;
            executor.CleanDestination = clean;
            executor.AsArchive = zip != null;

            await executor.ExecuteAsync(projectFile.DirectoryName, destPath, lifetime.Token);

            return 0;
        }
        catch (ApplicationException error) {
            ConsoleEx.Write("ERROR: ", ConsoleColor.Red);
            ConsoleEx.WriteLine(error.Message, ConsoleColor.DarkRed);
            return -1;
        }
        catch (Exception error) {
            logger.LogError(error, "An unhandled exception occurred while publishing!");
            return -1;
        }
    }

    internal class Executor
    {
        private readonly IServiceBuilder serviceBuilder;

        public ProjectPublishContext Context {get; set;}
        public bool CleanDestination {get; set;}
        public int Concurrency {get; set;} = 1;
        public bool AsArchive {get; set;}


        public Executor(IServiceBuilder serviceBuilder)
        {
            this.serviceBuilder = serviceBuilder;

            serviceBuilder.Initialize();
        }

        public async Task ExecuteAsync(string sourcePath, string destFilename, CancellationToken token = default)
        {
            if (sourcePath == null) throw new ApplicationException("Source path must be defined!");
            if (destFilename == null) throw new ApplicationException("Either Destination or Zip must be defined!");

            var timer = Stopwatch.StartNew();

            var contentType = AsArchive ? ContentTypes.Archive : ContentTypes.File;
            var edition = GameEdition.Parse(Context.Profile.Edition);
                
            serviceBuilder.ConfigureReader(ContentTypes.File, GameEditions.None, sourcePath);
            serviceBuilder.ConfigureWriter(contentType, edition, destFilename);
            serviceBuilder.Services.AddTransient<Executor>();
            serviceBuilder.AddPublisher(edition);

            serviceBuilder.Services.AddLogging(builder => builder.AddSerilog());

            await using var scope = serviceBuilder.Build();

            var writer = scope.GetRequiredService<IOutputWriter>();
            writer.Prepare();

            var publisher = scope.GetRequiredService<IPublisher>();
            publisher.Concurrency = Concurrency;

            try {
                await publisher.PrepareAsync(Context, CleanDestination, token);
                await publisher.PublishAsync(Context, token);
            }
            finally {
                timer.Stop();

                var summary = scope.GetRequiredService<IPublishSummary>();
                ConsoleEx.WriteLine("\nPublished", ConsoleColor.White);
                ConsoleEx.Write("  Duration    : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(UnitHelper.GetReadableTimespan(timer.Elapsed), ConsoleColor.Cyan);
                ConsoleEx.Write("  # Materials : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(summary.MaterialCount.ToString("N0"), ConsoleColor.Cyan);
                ConsoleEx.Write("  # Textures  : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(summary.TextureCount.ToString("N0"), ConsoleColor.Cyan);
                ConsoleEx.Write("  Disk Size   : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(summary.DiskSize, ConsoleColor.Cyan);
                ConsoleEx.Write("  Tex Memory  : ", ConsoleColor.Gray);
                ConsoleEx.WriteLine(summary.RawSize, ConsoleColor.Cyan);
            }
        }
    }
}