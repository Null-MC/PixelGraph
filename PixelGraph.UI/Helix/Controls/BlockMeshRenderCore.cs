using HelixToolkit.SharpDX.Core;
using HelixToolkit.SharpDX.Core.Core;
using HelixToolkit.SharpDX.Core.Render;

namespace PixelGraph.UI.Helix.Controls
{
    internal class BlockMeshRenderCore : MeshRenderCore
    {
        protected override void OnRenderShadow(RenderContext context, DeviceContextProxy deviceContext)
        {
            //...

            //base.OnRenderShadow(context, deviceContext);
            var pass = MaterialVariables.GetShadowPass(RenderType, context);

            if (pass.IsNULL) return;

            var v = new SimpleMeshStruct {
                World = ModelMatrix,
                HasInstances = InstanceBuffer.HasElements ? 1 : 0
            };

            if (!MaterialVariables.UpdateNonMaterialStruct(deviceContext, ref v, SimpleMeshStruct.SizeInBytes)) return;

            pass.BindShader(deviceContext);
            pass.BindStates(deviceContext, ShadowStateBinding);

            if (!MaterialVariables.BindMaterialResources(context, deviceContext, pass)) return;

            MaterialVariables.Draw(deviceContext, GeometryBuffer, InstanceBuffer.ElementCount);
        }
    }
}
