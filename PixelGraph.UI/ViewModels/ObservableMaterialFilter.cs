using PixelGraph.Common.Material;
using PixelGraph.UI.Internal;

namespace PixelGraph.UI.ViewModels;

public class ObservableMaterialFilter : ModelBase
{
    public MaterialFilter Filter {get;}

    public string Name {
        get => Filter.Name;
        set {
            Filter.Name = value;
            OnPropertyChanged();
        }
    }

    public decimal? Left {
        get => Filter.Left;
        set => Filter.Left = value;
    }

    public decimal? Top {
        get => Filter.Top;
        set => Filter.Top = value;
    }

    public decimal? Width {
        get => Filter.Width;
        set => Filter.Width = value;
    }

    public decimal? Height {
        get => Filter.Height;
        set => Filter.Height = value;
    }

    public bool? Tile {
        get => Filter.Tile;
        set => Filter.Tile = value;
    }

    public decimal? NormalNoise {
        get => Filter.NormalNoise;
        set => Filter.NormalNoise = value;
    }

    public decimal? NormalCurveX {
        get => Filter.NormalCurveX;
        set => Filter.NormalCurveX = value;
    }

    public decimal? NormalCurveLeft {
        get => Filter.NormalCurveLeft;
        set => Filter.NormalCurveLeft = value;
    }

    public decimal? NormalCurveRight {
        get => Filter.NormalCurveRight;
        set => Filter.NormalCurveRight = value;
    }

    public decimal? NormalCurveY {
        get => Filter.NormalCurveY;
        set => Filter.NormalCurveY = value;
    }

    public decimal? NormalCurveTop {
        get => Filter.NormalCurveTop;
        set => Filter.NormalCurveTop = value;
    }

    public decimal? NormalCurveBottom {
        get => Filter.NormalCurveBottom;
        set => Filter.NormalCurveBottom = value;
    }

    public decimal? NormalRadiusX {
        get => Filter.NormalRadiusX;
        set => Filter.NormalRadiusX = value;
    }

    public decimal? NormalRadiusLeft {
        get => Filter.NormalRadiusLeft;
        set => Filter.NormalRadiusLeft = value;
    }

    public decimal? NormalRadiusRight {
        get => Filter.NormalRadiusRight;
        set => Filter.NormalRadiusRight = value;
    }

    public decimal? NormalRadiusY {
        get => Filter.NormalRadiusY;
        set => Filter.NormalRadiusY = value;
    }

    public decimal? NormalRadiusTop {
        get => Filter.NormalRadiusTop;
        set => Filter.NormalRadiusTop = value;
    }

    public decimal? NormalRadiusBottom {
        get => Filter.NormalRadiusBottom;
        set => Filter.NormalRadiusBottom = value;
    }


    public ObservableMaterialFilter(MaterialFilter filter)
    {
        Filter = filter;
    }

    public void NotifyPropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}