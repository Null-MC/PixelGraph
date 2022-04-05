using PixelGraph.Common.ConnectedTextures;
using PixelGraph.Common.Material;
using PixelGraph.UI.Internal;
using PixelGraph.UI.Models.PropertyGrid;
using PixelGraph.UI.ViewData;
using PixelGraph.UI.ViewModels;
using System;
using System.ComponentModel;

namespace PixelGraph.UI.Models
{
    internal class MaterialConnectionsModel : ModelBase
    {
        private MaterialProperties _material;

        public event EventHandler DataChanged;

        public PrimaryConnectionPropertyCollection PrimaryProperties {get;}
        public CTMPropertyCollection ConnectionProperties {get;}

        public bool HasMaterial => _material != null;
        public bool IsMethodNotNone => _material?.CTM?.Method != null;

        public MaterialProperties Material {
            get => _material;
            set {
                if (_material == value) return;

                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                OnPropertyChanged(nameof(IsMethodNotNone));

                if (_material != null)
                    _material.CTM ??= new MaterialConnectionProperties();

                PrimaryProperties.SetData(_material?.CTM);
                ConnectionProperties.SetData(_material);
            }
        }


        public MaterialConnectionsModel()
        {
            PrimaryProperties = new PrimaryConnectionPropertyCollection();
            PrimaryProperties.PropertyChanged += OnPrimaryPropertyValueChanged;

            ConnectionProperties = new CTMPropertyCollection();
            ConnectionProperties.PropertyChanged += OnPropertyValueChanged;
        }

        private void OnPrimaryPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();

            if (string.Equals(nameof(MaterialConnectionProperties.Method), e.PropertyName))
                OnPropertyChanged(nameof(IsMethodNotNone));
        }

        private void OnPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();
        }

        private void OnDataChanged()
        {
            DataChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class PrimaryConnectionPropertyCollection : PropertyCollectionBase<MaterialConnectionProperties>
    {
        public PrimaryConnectionPropertyCollection()
        {
            var methodValues = new CtmTypeValues();
            var methodOptions = new SelectPropertyRowOptions(methodValues, "Text", "Value");
            AddSelect<string>("Method", nameof(MaterialConnectionProperties.Method), methodOptions);
        }
    }

    public class CTMPropertyCollection : PropertyCollectionBase<MaterialConnectionProperties>
    {
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> widthRow;
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> heightRow;
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> matchTilesRow;


        public CTMPropertyCollection()
        {
            widthRow = AddValue<int?>("Width", nameof(MaterialConnectionProperties.Width), 1);
            heightRow = AddValue<int?>("Height", nameof(MaterialConnectionProperties.Height), 1);
            AddText<string>("Match Blocks", nameof(MaterialConnectionProperties.MatchBlocks));
            matchTilesRow = AddText<string>("Match Tiles", nameof(MaterialConnectionProperties.MatchTiles));
            
            AddBool("Placeholder", nameof(MaterialConnectionProperties.Placeholder), false);
        }

        public void SetData(MaterialProperties material)
        {
            base.SetData(material?.CTM);

            var hasMatchBlocks = !string.IsNullOrWhiteSpace(material?.CTM?.MatchBlocks);
            matchTilesRow.DefaultValue = !hasMatchBlocks ? material?.Name : null;

            var bounds = material?.CTM != null ? CtmTypes.GetBounds(material.CTM) : null;
            widthRow.DefaultValue = bounds?.Width ?? 1;
            heightRow.DefaultValue = bounds?.Height ?? 1;

            var isFixedSize = CtmTypes.IsFixedSize(material?.CTM?.Method);
            widthRow.Enabled = heightRow.Enabled = !isFixedSize;
        }
    }
}
