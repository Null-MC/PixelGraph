using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class EnchantingTableBook : JavaEntityData
    {
        public const string EntityId = "enchanting_table_book";
        public const string EntityName = "Enchanting Table Book";


        public EnchantingTableBook() : base(EntityName)
        {
            Versions.Add(new JavaEntityDataVersion {
                Id = EntityId,
                TextVersion = "1.0",
                UVMappings = {
                    new UVRegion {
                        Name = "Cover-Right-Front",
                        Left = 0 /64d,
                        Top = 0 /32d,
                        Width = 6 /64d,
                        Height = 10 /32d,
                    },
                    new UVRegion {
                        Name = "Cover-Right-Back",
                        Left = 6 /64d,
                        Top = 0 /32d,
                        Width = 6 /64d,
                        Height = 10 /32d,
                    },

                    new UVRegion {
                        Name = "Cover-Left-Front",
                        Left = 16 /64d,
                        Top = 0 /32d,
                        Width = 6 /64d,
                        Height = 10 /32d,
                    },
                    new UVRegion {
                        Name = "Cover-Left-Back",
                        Left = 22 /64d,
                        Top = 0 /32d,
                        Width = 6 /64d,
                        Height = 10 /32d,
                    },

                    new UVRegion {
                        Name = "Spine-Back",
                        Left = 12 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 10 /32d,
                    },
                    new UVRegion {
                        Name = "Spine-Front",
                        Left = 14 /64d,
                        Top = 0 /32d,
                        Width = 2 /64d,
                        Height = 10 /32d,
                    },

                    new UVRegion {
                        Name = "Pages-Right-Down",
                        Left = 1 /64d,
                        Top = 10 /32d,
                        Width = 5 /64d,
                        Height = 1 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Right-Up",
                        Left = 6 /64d,
                        Top = 10 /32d,
                        Width = 5 /64d,
                        Height = 1 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Right-East",
                        Left = 0 /64d,
                        Top = 11 /32d,
                        Width = 1 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Right-South",
                        Left = 1 /64d,
                        Top = 11 /32d,
                        Width = 5 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Right-West",
                        Left = 6 /64d,
                        Top = 11 /32d,
                        Width = 1 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Right-North",
                        Left = 7 /64d,
                        Top = 11 /32d,
                        Width = 5 /64d,
                        Height = 8 /32d,
                    },

                    new UVRegion {
                        Name = "Pages-Left-Down",
                        Left = 13 /64d,
                        Top = 10 /32d,
                        Width = 5 /64d,
                        Height = 1 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Left-Up",
                        Left = 18 /64d,
                        Top = 10 /32d,
                        Width = 5 /64d,
                        Height = 1 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Left-East",
                        Left = 12 /64d,
                        Top = 11 /32d,
                        Width = 1 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Left-South",
                        Left = 13 /64d,
                        Top = 11 /32d,
                        Width = 5 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Left-West",
                        Left = 18 /64d,
                        Top = 11 /32d,
                        Width = 1 /64d,
                        Height = 8 /32d,
                    },
                    new UVRegion {
                        Name = "Pages-Left-North",
                        Left = 19 /64d,
                        Top = 11 /32d,
                        Width = 5 /64d,
                        Height = 8 /32d,
                    },
                },
            });
        }
    }
}
