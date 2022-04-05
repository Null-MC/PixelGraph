using Microsoft.Extensions.DependencyInjection;
using PixelGraph.Common;
using PixelGraph.Common.IO.Serialization;
using PixelGraph.Common.Material;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PixelGraph.UI.Internal.Utilities
{
    internal class ConcurrentMaterialWriter
    {
        private readonly ILogger<ConcurrentMaterialWriter> logger;
        private readonly IServiceProvider provider;
        private readonly IProjectContext projectContext;

        private readonly object lockHandle;
        //private readonly SemaphoreSlim locker;
        private MaterialProperties lastMaterial;


        public ConcurrentMaterialWriter(
            ILogger<ConcurrentMaterialWriter> logger,
            IServiceProvider provider,
            IProjectContext projectContext)
        {
            this.provider = provider;
            this.projectContext = projectContext;
            this.logger = logger;

            //locker = new SemaphoreSlim(1);
            lockHandle = new object();
        }

        private Task prevSaveTask;

        public Task SaveAsync(MaterialProperties material, CancellationToken token = default)
        {
            Task task = null;
            lock (lockHandle) {
                if (material == lastMaterial && prevSaveTask != null && !prevSaveTask.IsCompleted) {
                    task = prevSaveTask;
                    prevSaveTask = prevSaveTask.ContinueWith(t => InternalSaveAsync(material, projectContext.RootDirectory, token), token);
                }

                lastMaterial = material;
                prevSaveTask = Task.Run(() => InternalSaveAsync(material, projectContext.RootDirectory, token), token);
            }

            return task ?? Task.CompletedTask;
        }

        public async Task FlushPendingTasks()
        {
            Task task;
            lock (lockHandle) {
                task = prevSaveTask;
            }

            if (task != null) await task;
        }

        private async Task InternalSaveAsync(MaterialProperties material, string rootPath, CancellationToken token)
        {
            var serviceBuilder = provider.GetRequiredService<IServiceBuilder>();
            
            serviceBuilder.Initialize();
            serviceBuilder.ConfigureWriter(ContentTypes.File, GameEditions.None, rootPath);

            await using var scope = serviceBuilder.Build();

            try {
                var matWriter = scope.GetRequiredService<IMaterialWriter>();
                await matWriter.WriteAsync(material, token);
            }
            catch (Exception error) {
                //throw new ApplicationException($"Failed to save material '{material.LocalFilename}'!", error);
                logger.LogError(error, "Failed to save material properties file '{LocalFilename}'!", material.LocalFilename);
            }
        }
    }
}
