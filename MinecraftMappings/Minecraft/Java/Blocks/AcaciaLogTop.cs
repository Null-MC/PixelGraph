﻿using MinecraftMappings.Internal;
using BedrockBlocks = MinecraftMappings.Minecraft.Bedrock.Blocks;

namespace MinecraftMappings.Minecraft.Java.Blocks
{
    public class AcaciaLogTop : JavaBlockData
    {
        public const string BlockId = "acacia_log_top";
        public const string BlockName = "Acacia Log Top";


        public AcaciaLogTop() : base(BlockName)
        {
            Versions.Add(new JavaBlockDataVersion {
                Id = BlockId,
                MapsToBedrockId = BedrockBlocks.LogAcaciaTop.BlockId,
            });
        }
    }
}
