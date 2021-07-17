using System;
using System.Collections.Generic;
using System.Linq;

namespace MinecraftMappings.Internal
{
    public abstract class MinecraftGameEdition<TBlock, TBlockVersion, TEntity, TEntityVersion>
        where TBlock : BlockData<TBlockVersion>
        where TEntity : EntityData<TEntityVersion>
        where TBlockVersion : BlockDataVersion, new()
        where TEntityVersion : EntityDataVersion
    {
        private static readonly Lazy<IEnumerable<TBlock>> allBlocksLazy = new Lazy<IEnumerable<TBlock>>(BlockData<TBlockVersion>.FindBlockData<TBlock>);
        private static readonly Lazy<IEnumerable<TEntity>> allEntitiesLazy = new Lazy<IEnumerable<TEntity>>(EntityData<TEntityVersion>.FindEntityData<TEntity>);

        public IEnumerable<TBlock> AllBlocks => allBlocksLazy.Value;
        public IEnumerable<TEntity> AllEntities => allEntitiesLazy.Value;

        public IEnumerable<TBlockVersion> FindBlockById(string id)
        {
            return allBlocksLazy.Value
                .Select(block => block.GetLatestVersion())
                .Where(latest => latest.Id.Equals(id));
        }

        public IEnumerable<TEntityVersion> FindEntityById(string id)
        {
            return allEntitiesLazy.Value
                .Select(block => block.GetLatestVersion())
                .Where(latest => latest.Id.Equals(id));
        }
    }
}
