using MinecraftMappings.Internal;
using MinecraftMappings.Internal.Entities;
using MinecraftMappings.Internal.Models;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class ChestLargeRight : JavaEntityData
    {
        public const string EntityId = "normal_right";
        public const string EntityName = "Chest (Large Right)";


        public ChestLargeRight() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.0",
                Path = "textures/entity/chest/",
                UVMappings = {
                    new UVRegion {
                        Name = "Base-Down",
                        Left = 14 /64d,
                        Top = 19 /64d,
                        Width = 15 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Base-Up",
                        Left = 29 /64d,
                        Top = 19 /64d,
                        Width = 15 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Base-East",
                        Left = 0 /64d,
                        Top = 33 /64d,
                        Width = 14 /64d,
                        Height = 10 /64d,
                    },
                    new UVRegion {
                        Name = "Base-South",
                        Left = 14 /64d,
                        Top = 33 /64d,
                        Width = 15 /64d,
                        Height = 10 /64d,
                    },
                    new UVRegion {
                        Name = "Base-North",
                        Left = 43 /64d,
                        Top = 33 /64d,
                        Width = 15 /64d,
                        Height = 10 /64d,
                    },

                    new UVRegion {
                        Name = "Lid-Down",
                        Left = 14 /64d,
                        Top = 0 /64d,
                        Width = 15 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-Up",
                        Left = 29 /64d,
                        Top = 0 /64d,
                        Width = 15 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-East",
                        Left = 0 /64d,
                        Top = 14 /64d,
                        Width = 14 /64d,
                        Height = 5 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-South",
                        Left = 14 /64d,
                        Top = 14 /64d,
                        Width = 15 /64d,
                        Height = 5 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-North",
                        Left = 43 /64d,
                        Top = 14 /64d,
                        Width = 15 /64d,
                        Height = 5 /64d,
                    },

                    new UVRegion {
                        Name = "Knob-Down",
                        Left = 1 /64d,
                        Top = 0 /64d,
                        Width = 1 /64d,
                        Height = 1 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-Up",
                        Left = 2 /64d,
                        Top = 0 /64d,
                        Width = 1 /64d,
                        Height = 1 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-East",
                        Left = 0 /64d,
                        Top = 1 /64d,
                        Width = 1 /64d,
                        Height = 4 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-South",
                        Left = 1 /64d,
                        Top = 1 /64d,
                        Width = 1 /64d,
                        Height = 4 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-North",
                        Left = 3 /64d,
                        Top = 1 /64d,
                        Width = 1 /64d,
                        Height = 4 /64d,
                    },
                },
            });
        }
    }
}
