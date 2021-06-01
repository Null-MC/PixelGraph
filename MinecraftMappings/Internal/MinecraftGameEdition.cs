using System;
using System.Collections.Generic;

namespace MinecraftMappings.Internal
{
    public abstract class MinecraftGameEdition<TBlock, TBlockVersion, TEntity, TEntityVersion>
        where TBlock : BlockData<TBlockVersion>
        where TEntity : EntityData<TEntityVersion>
        where TBlockVersion : BlockDataVersion
        where TEntityVersion : EntityDataVersion
    {
        private static readonly Lazy<IEnumerable<TBlock>> allBlocksLazy = new Lazy<IEnumerable<TBlock>>(BlockData<TBlockVersion>.FindBlockData<TBlock>);
        private static readonly Lazy<IEnumerable<TEntity>> allEntitiesLazy = new Lazy<IEnumerable<TEntity>>(EntityData<TEntityVersion>.FindEntityData<TEntity>);

        public IEnumerable<TBlock> AllBlocks => allBlocksLazy.Value;
        public IEnumerable<TEntity> AllEntities => allEntitiesLazy.Value;
    }
}
