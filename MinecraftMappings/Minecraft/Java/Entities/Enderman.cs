using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Entities;
using MinecraftMappings.Internal.Models;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class Enderman : JavaEntityData
    {
        public const string EntityId = "enderman";
        public const string EntityName = "Enderman";


        public Enderman() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.8",
                Path = "textures/entity/enderman",
                UVMappings = {
                    new UVRegion {
                        Name = "Head-Up",
                        Left = 8 /64d,
                        Top = 0 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Head-Down",
                        Left = 16 /64d,
                        Top = 0 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Head-East",
                        Left = 0 /64d,
                        Top = 8 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Head-North",
                        Left = 8 /64d,
                        Top = 8 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Head-West",
                        Left = 16 /64d,
                        Top = 8 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Head-South",
                        Left = 24 /64d,
                        Top = 8 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },

                    new UVRegion {
                        Name = "Headwear-Up",
                        Left = 8 /64d,
                        Top = 16 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Headwear-Down",
                        Left = 16 /64d,
                        Top = 16 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Headwear-East",
                        Left = 0 /64d,
                        Top = 24 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Headwear-North",
                        Left = 8 /64d,
                        Top = 24 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Headwear-West",
                        Left = 16 /64d,
                        Top = 24 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Headwear-South",
                        Left = 24 /64d,
                        Top = 24 /32d,
                        Width = 8 /64d,
                        Height = 8 /32d,
                    },

                    new UVRegion {
                        Name = "Body-Up",
                        Left = 36 /64d,
                        Top = 16 /32d,
                        Width = 8 /64d,
                        Height = 4 /32d,
                    },
                    new UVRegion {
                        Name = "Body-Down",
                        Left = 44 /64d,
                        Top = 16 /32d,
                        Width = 8 /64d,
                        Height = 4 /32d,
                    },
                    new UVRegion {
                        Name = "Body-East",
                        Left = 32 /64d,
                        Top = 20 /32d,
                        Width = 4 /64d,
                        Height = 12 /32d,
                    },
                    new UVRegion {
                        Name = "Body-North",
                        Left = 36 /64d,
                        Top = 20 /32d,
                        Width = 8 /64d,
                        Height = 12 /32d,
                    },
                    new UVRegion {
                        Name = "Body-West",
                        Left = 44 /64d,
                        Top = 20 /32d,
                        Width = 4 /64d,
                        Height = 12 /32d,
                    },
                    new UVRegion {
                        Name = "Body-South",
                        Left = 48 /64d,
                        Top = 20 /32d,
                        Width = 8 /64d,
                        Height = 12 /32d,
                    },

                    new UVRegion {
                        Name = "Limb-Up",
                        Left = 58 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 2 /32d,
                    },
                    new UVRegion {
                        Name = "Limb-Down",
                        Left = 60 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 2 /32d,
                    },
                    new UVRegion {
                        Name = "Limb-East",
                        Left = 56 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 30 /32d,
                    },
                    new UVRegion {
                        Name = "Limb-North",
                        Left = 58 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 30 /32d,
                    },
                    new UVRegion {
                        Name = "Limb-West",
                        Left = 60 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 30 /32d,
                    },
                    new UVRegion {
                        Name = "Limb-South",
                        Left = 62 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 30 /32d,
                    },
                },
            });
        }
    }
}
