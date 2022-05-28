using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common.Extensions;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Internal.Projects;
using PixelGraph.UI.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PixelGraph.UI.Internal.IO;

namespace PixelGraph.UI.ViewModels
{
    internal class NewProjectViewModel
    {
        private readonly IProjectContextManager projectContextMgr;
        private readonly IRecentPathManager recentMgr;

        public NewProjectModel Model {get; set;}


        public NewProjectViewModel(IServiceProvider provider)
        {
            projectContextMgr = provider.GetRequiredService<IProjectContextManager>();
            recentMgr = provider.GetRequiredService<IRecentPathManager>();
        }

        public async Task CreateProjectAsync(ProjectContext projectContext, CancellationToken token = default)
        {
            if (!Directory.Exists(projectContext.RootDirectory))
                Directory.CreateDirectory(projectContext.RootDirectory);

            projectContextMgr.SetContext(projectContext);
            await projectContextMgr.SaveAsync();

            recentMgr.Insert(projectContext.ProjectFilename);
            await recentMgr.SaveAsync(token);

            if (Model.CreateMinecraftFolders)
                CreateDefaultMinecraftFolders(projectContext);

            if (Model.CreateRealmsFolders)
                CreateDefaultRealmsFolders(projectContext);

            if (Model.CreateOptifineFolders)
                CreateDefaultOptifineFolders(projectContext);
        }

        private static void CreateDefaultMinecraftFolders(ProjectContext projectContext)
        {
            var mcPath = PathEx.Join(projectContext.RootDirectory, "assets", "minecraft");

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

            var realmsPath = PathEx.Join(projectContext.RootDirectory, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private static void CreateDefaultRealmsFolders(ProjectContext projectContext)
        {
            var realmsPath = PathEx.Join(projectContext.RootDirectory, "assets", "realms");

            TryCreateDirectory(realmsPath, "textures");
        }

        private static void CreateDefaultOptifineFolders(ProjectContext projectContext)
        {
            var optifinePath = PathEx.Join(projectContext.RootDirectory, "assets", "minecraft", "optifine");

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
