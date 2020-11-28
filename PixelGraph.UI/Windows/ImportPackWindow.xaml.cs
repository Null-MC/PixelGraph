using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Importing;
using PixelGraph.Common.ResourcePack;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PixelGraph.UI.Windows
{
    public partial class ImportPackWindow
    {
        private readonly IServiceProvider provider;


        public ImportPackWindow(IServiceProvider provider)
        {
            this.provider = provider;

            InitializeComponent();
        }

        public async Task RunAsync(CancellationToken token = default)
        {
            var scopeBuilder = provider.GetRequiredService<IServiceBuilder>();
            scopeBuilder.AddFileInput();
            scopeBuilder.AddFileOutput();

            await using var scope = scopeBuilder.Build();
            var importer = provider.GetRequiredService<IResourcePackImporter>();

            importer.AsGlobal = VM.AsGlobal;
            importer.CopyUntracked = VM.CopyUntracked;
            importer.PackInput = VM.PackInput;

            importer.PackProfile = new ResourcePackProfileProperties {
                Output = new ResourcePackOutputProperties {
                    Format = VM.SourceFormat,
                    //...
                },
            };

            await importer.ImportAsync(token);
        }
    }
}
