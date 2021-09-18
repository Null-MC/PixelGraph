using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftMappings.Internal
{
    public abstract class MinecraftGameEdition<TBlock, TItem, TEntity>
        where TBlock : IBlockData
        where TItem : IItemData
        where TEntity : IEntityData
    {
        private static readonly Lazy<IEnumerable<TBlock>> allBlocksLazy = new Lazy<IEnumerable<TBlock>>(BlockData.FromAssembly<TBlock>);
        private static readonly Lazy<IEnumerable<TItem>> allItemsLazy = new Lazy<IEnumerable<TItem>>(ItemData.FromAssembly<TItem>);
        private static readonly Lazy<IEnumerable<TEntity>> allEntitiesLazy = new Lazy<IEnumerable<TEntity>>(EntityData.FromAssembly<TEntity>);

        public IEnumerable<TBlock> AllBlocks => allBlocksLazy.Value;
        public IEnumerable<TItem> AllItems => allItemsLazy.Value;
        public IEnumerable<TEntity> AllEntities => allEntitiesLazy.Value;


        public IEnumerable<TBlockVersion> FindBlockVersionById<TBlockVersion>(string id)
            where TBlockVersion : BlockDataVersion, new()
        {
            return allBlocksLazy.Value.OfType<IBlockData<TBlockVersion>>()
                .Select(block => block.GetLatestVersion())
                .Where(latest => latest.Id.Equals(id));
        }

        public IEnumerable<TItemVersion> FindItemVersionById<TItemVersion>(string id)
            where TItemVersion : ItemDataVersion, new()
        {
            return allItemsLazy.Value.OfType<IItemData<TItemVersion>>()
                .Select(item => item.GetLatestVersion())
                .Where(latest => latest.Id.Equals(id));
        }

        public IEnumerable<TEntityVersion> FindEntityVersionById<TEntityVersion>(string id)
            where TEntityVersion : EntityDataVersion, new()
        {
            return allEntitiesLazy.Value.OfType<IEntityData<TEntityVersion>>()
                .Select(entity => entity.GetLatestVersion())
                .Where(latest => latest.Id.Equals(id));
        }

        public IEnumerable<TEntityVersion> GetEntitiesByVersion<TEntityVersion>(Version version)
            where TEntityVersion : EntityDataVersion, new()
        {
            return allEntitiesLazy.Value.OfType<IEntityData<TEntityVersion>>()
                .Select(entity => entity.GetVersion(version));
        }
    }
}
