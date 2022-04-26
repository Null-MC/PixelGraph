using PixelGraph.UI.Models;
using System;

namespace PixelGraph.UI.ViewModels
{
    internal class PackInputViewModel
    {
        private readonly IServiceProvider provider;

        public PackInputModel Model {get; set;}


        public PackInputViewModel(IServiceProvider provider)
        {
            this.provider = provider;
        }

        //public async Task SavePackInputAsync()
        //{
        //    var projectContext = provider.GetRequiredService<IProjectContext>();
        //    var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

        //    serviceBuilder.Initialize();
        //    serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, projectContext.RootDirectory);

        //    await using var scope = serviceBuilder.Build();

        //    try {
        //        var packWriter = scope.GetRequiredService<IResourcePackWriter>();
        //        await packWriter.WriteAsync("input.yml", Model.PackInput);
        //    }
        //    catch (Exception error) {
        //        throw new ApplicationException("Failed to save pack input!", error);
        //    }
        //}
    }
}
