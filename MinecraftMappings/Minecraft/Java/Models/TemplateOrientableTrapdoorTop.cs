using MinecraftMappings.Internal.Models;
using SharpDX;

namespace MinecraftMappings.Minecraft.Java.Models
{
    public class TemplateOrientableTrapdoorTop : JavaModelData
    {
        public TemplateOrientableTrapdoorTop() : base("Template Orientable Trapdoor Top")
        {
            AddVersion("template_orientable_trapdoor_top", "1.0.0")
                .WithPath("models/armor/iron_layer_1")
                //.AddTexture("particle", "#texture")
                .AddElement(element => {
                    element.From = new Vector3(0f, 13f, 0f);
                    element.To = new Vector3(16f, 16f, 16f);
                    element.FaceUp = new ModelFace {
                        UV = new RectangleF(0f, 16f, 16f, 0f),
                        Texture = "#texture",
                    };
                    element.FaceDown = new ModelFace {
                        UV = new RectangleF(0f, 0f, 16f, 16f),
                        Texture = "#texture",
                    };
                    element.FaceNorth = new ModelFace {
                        UV = new RectangleF(0f, 0f, 16f, 3f),
                        Texture = "#texture",
                    };
                    element.FaceSouth = new ModelFace {
                        UV = new RectangleF(0f, 0f, 16f, 3f),
                        Texture = "#texture",
                    };
                    element.FaceWest = new ModelFace {
                        UV = new RectangleF(0f, 0f, 16f, 3f),
                        Texture = "#texture",
                    };
                    element.FaceEast = new ModelFace {
                        UV = new RectangleF(0f, 0f, 16f, 3f),
                        Texture = "#texture",
                    };
                });
        }
    }
}
