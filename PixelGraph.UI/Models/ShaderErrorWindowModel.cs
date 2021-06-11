using PixelGraph.UI.Internal.Preview.Shaders;

namespace PixelGraph.UI.Models
{
    public class ShaderErrorWindowModel
    {
        public ShaderCompileError[] Errors {get; set;}
    }

    public class ShaderErrorWindowDesignerModel : ShaderErrorWindowModel
    {
        public ShaderErrorWindowDesignerModel()
        {
            Errors = new[] {
                new ShaderCompileError {
                    Filename = "C:\\dev\\shaders\\file.hlsl",
                    Message = "This file sucks!",
                },
            };
        }
    }
}
