using System;
using System.Collections.Generic;

namespace PixelGraph.UI.Internal.Tabs
{
    public interface ITabPreviewManager : IDisposable
    {
        IEnumerable<TabPreviewContext> All {get;}

        void Add(TabPreviewContext context);
        TabPreviewContext Get(Guid id);
        void Remove(Guid id);
        void Clear();
        void InvalidateAll(bool clear);
        void InvalidateAllLayers(bool clear);

#if !NORENDER
        void InvalidateAllMaterialBuilders(bool clear);
        void InvalidateAllMaterials(bool clear);
#endif
    }

    public class TabPreviewManager : ITabPreviewManager
    {
        private readonly Dictionary<Guid, TabPreviewContext> contextMap;

        public IEnumerable<TabPreviewContext> All => contextMap.Values;


        public TabPreviewManager()
        {
            contextMap = new Dictionary<Guid, TabPreviewContext>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Add(TabPreviewContext context)
        {
            contextMap[context.Id] = context;
        }

        public TabPreviewContext Get(Guid id)
        {
            return contextMap.TryGetValue(id, out var result) ? result : null;
        }

        public void Remove(Guid id)
        {
            contextMap.Remove(id);
        }

        public void Clear()
        {
            foreach (var context in All) context.Dispose();
            contextMap.Clear();
        }

        public void InvalidateAll(bool clear)
        {
            foreach (var context in All) {
                context.InvalidateLayer(clear);

#if !NORENDER
                context.InvalidateMaterialBuilder(clear);
#endif
            }
        }

        public void InvalidateAllLayers(bool clear)
        {
            foreach (var context in All)
                context.InvalidateLayer(clear);
        }

#if !NORENDER
        public void InvalidateAllMaterialBuilders(bool clear)
        {
            foreach (var context in All)
                context.InvalidateMaterialBuilder(clear);
        }

        public void InvalidateAllMaterials(bool clear)
        {
            foreach (var context in All) {
                context.InvalidateMaterial(clear);

                if (clear) {
                    context.Mesh.ClearTextureBuilders();
                }
            }
        }
#endif

        protected virtual void Dispose(bool disposing)
        {
            Clear();
        }
    }
}
