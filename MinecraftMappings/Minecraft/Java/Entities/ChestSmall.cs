using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class ChestSmall : JavaEntityData
    {
        public const string EntityId = "normal";
        public const string EntityName = "Chest (Small)";


        public ChestSmall() : base(EntityName)
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
                        Width = 14 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Base-Up",
                        Left = 28 /64d,
                        Top = 19 /64d,
                        Width = 14 /64d,
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
                        Width = 14 /64d,
                        Height = 10 /64d,
                    },
                    new UVRegion {
                        Name = "Base-West",
                        Left = 28 /64d,
                        Top = 33 /64d,
                        Width = 14 /64d,
                        Height = 10 /64d,
                    },
                    new UVRegion {
                        Name = "Base-North",
                        Left = 42 /64d,
                        Top = 33 /64d,
                        Width = 14 /64d,
                        Height = 10 /64d,
                    },

                    new UVRegion {
                        Name = "Lid-Down",
                        Left = 14 /64d,
                        Top = 0 /64d,
                        Width = 14 /64d,
                        Height = 14 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-Up",
                        Left = 28 /64d,
                        Top = 0 /64d,
                        Width = 14 /64d,
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
                        Width = 14 /64d,
                        Height = 5 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-West",
                        Left = 28 /64d,
                        Top = 14 /64d,
                        Width = 14 /64d,
                        Height = 5 /64d,
                    },
                    new UVRegion {
                        Name = "Lid-North",
                        Left = 42 /64d,
                        Top = 14 /64d,
                        Width = 14 /64d,
                        Height = 5 /64d,
                    },

                    new UVRegion {
                        Name = "Knob-Down",
                        Left = 1 /64d,
                        Top = 0 /64d,
                        Width = 2 /64d,
                        Height = 1 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-Up",
                        Left = 3 /64d,
                        Top = 0 /64d,
                        Width = 2 /64d,
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
                        Width = 2 /64d,
                        Height = 4 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-West",
                        Left = 3 /64d,
                        Top = 1 /64d,
                        Width = 1 /64d,
                        Height = 4 /64d,
                    },
                    new UVRegion {
                        Name = "Knob-North",
                        Left = 4 /64d,
                        Top = 1 /64d,
                        Width = 2 /64d,
                        Height = 4 /64d,
                    },
                },
            });
        }
    }
}
