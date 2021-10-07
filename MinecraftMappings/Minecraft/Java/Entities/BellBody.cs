using MinecraftMappings.Internal.Entities;
using SharpDX;

namespace MinecraftMappings.Minecraft.Java.Entities
{
    public class BellBody : JavaEntityData
    {
        public const string EntityId = "bell_body";
        public const string EntityName = "Bell Body";


        public BellBody() : base(EntityName)
        {
            AddVersion(EntityId, "1.0.0")
                .WithPath("bell")
                .AddElement("top", element => {
                    element.Position = new Vector3(-3f, -7f, -3f);
                    element.Size = new Vector3(6f, 7f, 6f);
                    element.UV = Vector2.Zero;
                })
                .AddElement("bottom", element => {
                    element.Position = new Vector3(-4f, 0f, -4f);
                    element.Size = new Vector3(8f, 2f, 8f);
                    element.UV = new Vector2(0f, 13f);
                });
        }
    }
}
