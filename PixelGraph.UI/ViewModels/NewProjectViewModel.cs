using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.Extensions;
using PixelGraph.Common.IO;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.ResourcePack;
using PixelGraph.Common.TextureFormats;
using PixelGraph.UI.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PixelGraph.UI.ViewModels
{
    internal class NewProjectViewModel
    {
        private readonly IServiceProvider provider;

        public NewProjectModel Model {get; set;}


        public NewProjectViewModel(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public async Task CreatePackFilesAsync()
        {
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();

            serviceBuilder.Initialize();
            serviceBuilder.AddContentWriter(ContentTypes.File);
            serviceBuilder.Services.Configure<OutputOptions>(options => options.Root = Model.Location);

            await using var scope = serviceBuilder.Build();

            var packWriter = provider.GetRequiredService<IResourcePackWriter>();

            var packInput = new ResourcePackInputProperties {
                Format = TextureFormat.Format_Raw,
            };

            await packWriter.WriteAsync("input.yml", packInput);

            if (Model.CreateDefaultProfile) {
                var packProfile = new ResourcePackProfileProperties {
                    Name = Model.PackName,
                    Description = "A short description of the RP content.",
                    Encoding = {
                        Format = TextureFormat.Format_Lab13,
                    },
                    Edition = "Java",
                    Format = 6,
                };

                var safeName = Model.PackName
                    .Replace('/', '_')
                    .Replace('\\', '_');

                await packWriter.WriteAsync($"{safeName}.pack.yml", packProfile);
            }
        }

        public void CreateDirectories()
        {
            if (!Directory.Exists(Model.Location))
                Directory.CreateDirectory(Model.Location);

            if (Model.CreateMinecraftFolders)
                CreateDefaultMinecraftFolders();

            if (Model.CreateRealmsFolders)
                CreateDefaultRealmsFolders();

            if (Model.CreateOptifineFolders)
                CreateDefaultOptifineFolders();
        }

        private void CreateDefaultMinecraftFolders()
        {
            var mcPath = PathEx.Join(Model.Location, "assets", "minecraft");

            TryCreateDirectory(mcPath, "blockstates");
            TryCreateDirectory(mcPath, "font");
            TryCreateDirectory(mcPath, "models", "block");
            TryCreateDirectory(mcPath, "models", "item");
            TryCreateDirectory(mcPath, "particles");
            TryCreateDirectory(mcPath, "shaders");
            TryCreateDirectory(mcPath, "sounds");
            //TryCreateDirectory(mcPath, "sounds", "ambient");
            //TryCreateDirectory(mcPath, "sounds", "block");
            //TryCreateDirectory(mcPath, "sounds", "damage");
            //TryCreateDirectory(mcPath, "sounds", "dig");
            //TryCreateDirectory(mcPath, "sounds", "enchant");
            //TryCreateDirectory(mcPath, "sounds", "entity");
            //TryCreateDirectory(mcPath, "sounds", "fire");
            //TryCreateDirectory(mcPath, "sounds", "fireworks");
            //TryCreateDirectory(mcPath, "sounds", "item");
            //TryCreateDirectory(mcPath, "sounds", "liquid");
            //TryCreateDirectory(mcPath, "sounds", "minecart");
            //TryCreateDirectory(mcPath, "sounds", "mob");
            //TryCreateDirectory(mcPath, "sounds", "music");
            //TryCreateDirectory(mcPath, "sounds", "note");
            //TryCreateDirectory(mcPath, "sounds", "portal");
            //TryCreateDirectory(mcPath, "sounds", "random");
            //TryCreateDirectory(mcPath, "sounds", "records");
            //TryCreateDirectory(mcPath, "sounds", "step");
            //TryCreateDirectory(mcPath, "sounds", "tile");
            //TryCreateDirectory(mcPath, "sounds", "ui");
            TryCreateDirectory(mcPath, "texts");
            TryCreateDirectory(mcPath, "textures", "block");
            TryCreateDirectory(mcPath, "textures", "colormap");
            TryCreateDirectory(mcPath, "textures", "effect");
            TryCreateDirectory(mcPath, "textures", "entity");
            TryCreateDirectory(mcPath, "textures", "environment");
            TryCreateDirectory(mcPath, "textures", "font");
            TryCreateDirectory(mcPath, "textures", "gui");
            TryCreateDirectory(mcPath, "textures", "item");
            TryCreateDirectory(mcPath, "textures", "map");
            TryCreateDirectory(mcPath, "textures", "misc");
            TryCreateDirectory(mcPath, "textures", "mob_effect");
            TryCreateDirectory(mcPath, "textures", "models");
            TryCreateDirectory(mcPath, "textures", "painting");
            TryCreateDirectory(mcPath, "textures", "particle");

            var realmsPath = PathEx.Join(Model.Location, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private void CreateDefaultRealmsFolders()
        {
            var realmsPath = PathEx.Join(Model.Location, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private void CreateDefaultOptifineFolders()
        {
            var optifinePath = PathEx.Join(Model.Location, "assets", "minecraft", "optifine");

            TryCreateDirectory(optifinePath, "anim");
            TryCreateDirectory(optifinePath, "cem");
            TryCreateDirectory(optifinePath, "cit");
            TryCreateDirectory(optifinePath, "colormap");
            TryCreateDirectory(optifinePath, "ctm");
            TryCreateDirectory(optifinePath, "font");
            TryCreateDirectory(optifinePath, "gui");
            TryCreateDirectory(optifinePath, "lightmap");
            TryCreateDirectory(optifinePath, "mob");
            TryCreateDirectory(optifinePath, "random");
            TryCreateDirectory(optifinePath, "sky");
        }

        private static void TryCreateDirectory(params string[] parts)
        {
            var path = PathEx.Join(parts);
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }
    }
}
