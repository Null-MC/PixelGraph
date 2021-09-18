using MinecraftMappings.Internal;

namespace MinecraftMappings.Minecraft.Java.Models
{
    public abstract class Armor1 : JavaEntityData
    {
        protected static readonly UVRegion[] DefaultUVRegions = {
            new UVRegion {
                Name = "Head1-Up",
                Left = 8 /64d,
                Top = 0,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head1-Down",
                Left = 16 /64d,
                Top = 0,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head1-East",
                Left = 0,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head1-North",
                Left = 8 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head1-West",
                Left = 16 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head1-South",
                Left = 24 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },

            new UVRegion {
                Name = "Head1-Up",
                Left = 40 /64d,
                Top = 0,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head2-Down",
                Left = 48 /64d,
                Top = 0,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head2-East",
                Left = 32 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head2-North",
                Left = 40 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head2-West",
                Left = 48 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },
            new UVRegion {
                Name = "Head2-South",
                Left = 56 /64d,
                Top = 8 /32d,
                Width = 8 /64d,
                Height = 8 /32d,
            },

            new UVRegion {
                Name = "Body-Up",
                Left = 20 /64d,
                Top = 16 /32d,
                Width = 8 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Body-Down",
                Left = 28 /64d,
                Top = 16 /32d,
                Width = 8 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Body-East",
                Left = 16 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Body-North",
                Left = 20 /64d,
                Top = 20 /32d,
                Width = 8 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Body-West",
                Left = 28 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Body-South",
                Left = 32 /64d,
                Top = 20 /32d,
                Width = 8 /64d,
                Height = 12 /32d,
            },

            new UVRegion {
                Name = "Leg-Up",
                Left = 4 /64d,
                Top = 16 /32d,
                Width = 4 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Leg-Down",
                Left = 8 /64d,
                Top = 16 /32d,
                Width = 4 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Leg-East",
                Left = 0 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Leg-North",
                Left = 4 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Leg-West",
                Left = 8 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Leg-South",
                Left = 12 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },

            new UVRegion {
                Name = "Arm-Up",
                Left = 44 /64d,
                Top = 16 /32d,
                Width = 4 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Arm-Down",
                Left = 48 /64d,
                Top = 16 /32d,
                Width = 4 /64d,
                Height = 4 /32d,
            },
            new UVRegion {
                Name = "Arm-East",
                Left = 40 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Arm-North",
                Left = 44 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Arm-West",
                Left = 48 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
            new UVRegion {
                Name = "Arm-South",
                Left = 52 /64d,
                Top = 20 /32d,
                Width = 4 /64d,
                Height = 12 /32d,
            },
        };


        protected Armor1(string name) : base(name) {}
    }
}
