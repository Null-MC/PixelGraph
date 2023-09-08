using HelixToolkit.SharpDX.Core.Model.Scene;
using HelixToolkit.SharpDX.Core.Utilities;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model;
using PixelGraph.Rendering.LUTs;
using System.Windows;

namespace PixelGraph.UI.Helix.Controls;

public class DielectricBdrfLut3D : Element3D, ILutMapSource
{
    public int Resolution {
        get => (int)GetValue(ResolutionProperty);
        set => SetValue(ResolutionProperty, value);
    }

    private DielectricBdrfLutNode LutMapNode => SceneNode as DielectricBdrfLutNode;
    public ShaderResourceViewProxy LutMap => LutMapNode?.LutMap;
    public long LastUpdated => LutMapNode?.LastUpdated ?? 0;


    protected override SceneNode OnCreateSceneNode()
    {
        return new DielectricBdrfLutNode();
    }

    protected override void AssignDefaultValuesToSceneNode(SceneNode core)
    {
        if (core is DielectricBdrfLutNode n) {
            n.Resolution = Resolution;
        }

        base.AssignDefaultValuesToSceneNode(core);
    }

    public static readonly DependencyProperty ResolutionProperty =
        DependencyProperty.Register(nameof(Resolution), typeof(int), typeof(DielectricBdrfLut3D), new PropertyMetadata(256, (d, e) => {
            if (d is Element3DCore {SceneNode: DielectricBdrfLutNode sceneNode})
                sceneNode.Resolution = (int)e.NewValue;
        }));
}