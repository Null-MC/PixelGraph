using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.UI.Models;
using System;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInputViewModel
    {
        private readonly IOutputWriter writer;
        private readonly IResourcePackWriter packWriter;

        public PackInputModel Model {get; set;}


        public PackInputViewModel(IServiceProvider provider)
        {
            writer = provider.GetRequiredService<IOutputWriter>();
            packWriter = provider.GetRequiredService<IResourcePackWriter>();
        }

        public async Task SavePackInputAsync()
        {
            try {
                writer.SetRoot(Model.RootDirectory);
                await packWriter.WriteAsync("input.yml", Model.PackInput);
            }
            catch (Exception error) {
                throw new ApplicationException("Failed to save pack input!", error);
            }
        }
    }
}
