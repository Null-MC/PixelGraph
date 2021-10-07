using MinecraftMappings.Internal.Entities;
using SharpDX;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class Ghast : JavaEntityData
    {
        public Ghast() : base("Ghast")
        {
            AddVersion("ghast", "1.2.0")
                .WithPath("textures/entity/ghast")
                .AddElement("body", element => {
                    element.Position = new Vector3(-8f, 12f, -8f);
                    element.Size = new Vector3(16f, 16f, 16f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle1", element => {
                    element.Position = new Vector3(2.7f, 5f, -6f);
                    element.Size = new Vector3(2f, 8f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle2", element => {
                    element.Position = new Vector3(-7.3f, 4f, -6f);
                    element.Size = new Vector3(2f, 9f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle3", element => {
                    element.Position = new Vector3(-2.3f, 0f, -6f);
                    element.Size = new Vector3(2f, 13f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle4", element => {
                    element.Position = new Vector3(5.3f, 2f, -1f);
                    element.Size = new Vector3(2f, 11f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle5", element => {
                    element.Position = new Vector3(0.3f, 2f, -1f);
                    element.Size = new Vector3(2f, 11f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle6", element => {
                    element.Position = new Vector3(-4.7f, 3f, -1f);
                    element.Size = new Vector3(2f, 10f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle7", element => {
                    element.Position = new Vector3(2.7f, 1f, 4f);
                    element.Size = new Vector3(2f, 12f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle8", element => {
                    element.Position = new Vector3(-7.3f, 1f, 4f);
                    element.Size = new Vector3(2f, 12f, 2f);
                    element.UV = new Vector2(0f, 0f);
                })
                .AddElement("tentacle9", element => {
                    element.Position = new Vector3(-2.3f, 4f, 4f);
                    element.Size = new Vector3(2f, 9f, 2f);
                    element.UV = new Vector2(0f, 0f);
                });

            //Versions.Add(new JavaEntityDataVersion {
            //    Id = EntityId,
            //    TextVersion = "1.2.0",
            //    Path = ,
            //    UVMappings = {
            //        new UVRegion {
            //            Name = "Body-Up",
            //            Left = 16 /64d,
            //            Top = 0 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Body-Down",
            //            Left = 32 /64d,
            //            Top = 0 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Body-East",
            //            Left = 0 /64d,
            //            Top = 16 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Body-North",
            //            Left = 16 /64d,
            //            Top = 16 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Body-West",
            //            Left = 32 /64d,
            //            Top = 16 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Body-South",
            //            Left = 48 /64d,
            //            Top = 16 /32d,
            //            Width = 16 /64d,
            //            Height = 16 /32d,
            //        },

            //        new UVRegion {
            //            Name = "Tentacle-Up",
            //            Left = 2 /64d,
            //            Top = 0 /32d,
            //            Width = 2 /64d,
            //            Height = 2 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Tentacle-Down",
            //            Left = 4 /64d,
            //            Top = 0 /32d,
            //            Width = 2 /64d,
            //            Height = 2 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Tentacle-East",
            //            Left = 0 /64d,
            //            Top = 2 /32d,
            //            Width = 2 /64d,
            //            Height = 13 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Tentacle-North",
            //            Left = 2 /64d,
            //            Top = 2 /32d,
            //            Width = 2 /64d,
            //            Height = 13 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Tentacle-West",
            //            Left = 4 /64d,
            //            Top = 2 /32d,
            //            Width = 2 /64d,
            //            Height = 13 /32d,
            //        },
            //        new UVRegion {
            //            Name = "Tentacle-South",
            //            Left = 6 /64d,
            //            Top = 2 /32d,
            //            Width = 2 /64d,
            //            Height = 13 /32d,
            //        },
            //    },
            //});
        }
    }
}
