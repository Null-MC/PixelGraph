using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class Ghast : JavaEntityData
    {
        public const string EntityId = "ghast";
        public const string EntityName = "Ghast";


        public Ghast() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.2.0",
                Path = "textures/entity/ghast",
                UVMappings = {
                    new UVRegion {
                        Name = "Body-Up",
                        Left = 16 /64d,
                        Top = 0 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },
                    new UVRegion {
                        Name = "Body-Down",
                        Left = 32 /64d,
                        Top = 0 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },
                    new UVRegion {
                        Name = "Body-East",
                        Left = 0 /64d,
                        Top = 16 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },
                    new UVRegion {
                        Name = "Body-North",
                        Left = 16 /64d,
                        Top = 16 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },
                    new UVRegion {
                        Name = "Body-West",
                        Left = 32 /64d,
                        Top = 16 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },
                    new UVRegion {
                        Name = "Body-South",
                        Left = 48 /64d,
                        Top = 16 /32d,
                        Width = 16 /64d,
                        Height = 16 /32d,
                    },

                    new UVRegion {
                        Name = "Tentacle-Up",
                        Left = 2 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 2 /32d,
                    },
                    new UVRegion {
                        Name = "Tentacle-Down",
                        Left = 4 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 2 /32d,
                    },
                    new UVRegion {
                        Name = "Tentacle-East",
                        Left = 0 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 13 /32d,
                    },
                    new UVRegion {
                        Name = "Tentacle-North",
                        Left = 2 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 13 /32d,
                    },
                    new UVRegion {
                        Name = "Tentacle-West",
                        Left = 4 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 13 /32d,
                    },
                    new UVRegion {
                        Name = "Tentacle-South",
                        Left = 6 /64d,
                        Top = 2 /32d,
                        Width = 2 /64d,
                        Height = 13 /32d,
                    },
                },
            });
        }
    }
}
