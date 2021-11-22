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
        //private MaterialPart _selectedPart;

        public event EventHandler DataChanged;

        //public ObservableCollection<MaterialPart> PartsList {get;}
        public PrimaryConnectionPropertyCollection PrimaryProperties {get;}
        //public MultiPartPropertyCollection MultiPartProperties {get;}
        public CTMPropertyCollection ConnectionProperties {get;}

        public bool HasMaterial => _material != null;
        //public bool HasSelectedPart => _selectedPart != null;
        //public bool IsTypeMultiPart => TextureConnectionTypes.Is(TextureConnectionTypes.MultiPart, _material?.CTM?.Type);
        //public bool IsTypeCTM => _material?.CTM?.Type != null && !IsTypeMultiPart;
        public bool IsMethodNotNone => _material?.CTM?.Method != null;

        public MaterialProperties Material {
            get => _material;
            set {
                if (_material == value) return;

                _material = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasMaterial));

                //OnPropertyChanged(nameof(IsTypeMultiPart));
                //OnPropertyChanged(nameof(IsTypeCTM));
                OnPropertyChanged(nameof(IsMethodNotNone));

                if (_material != null)
                    _material.CTM ??= new MaterialConnectionProperties();

                PrimaryProperties.SetData(_material?.CTM);
                ConnectionProperties.SetData(_material);

                //SelectedPart = null;
                //UpdatePartsList();
            }
        }

        //public MaterialPart SelectedPart {
        //    get => _selectedPart;
        //    set {
        //        if (_selectedPart == value) return;

        //        _selectedPart = value;
        //        OnPropertyChanged();
        //        OnPropertyChanged(nameof(HasSelectedPart));

        //        MultiPartProperties.SetData(_selectedPart);
        //    }
        //}


        public MaterialConnectionsModel()
        {
            //PartsList = new ObservableCollection<MaterialPart>();

            PrimaryProperties = new PrimaryConnectionPropertyCollection();
            PrimaryProperties.PropertyChanged += OnPrimaryPropertyValueChanged;

            //MultiPartProperties = new MultiPartPropertyCollection();
            //MultiPartProperties.PropertyChanged += OnPropertyValueChanged;

            ConnectionProperties = new CTMPropertyCollection();
            ConnectionProperties.PropertyChanged += OnPropertyValueChanged;
        }

        //private void UpdatePartsList()
        //{
        //    PartsList.Clear();

        //    if (Material?.Parts == null) return;
        //    foreach (var part in Material.Parts) PartsList.Add(part);
        //}

        private void OnPrimaryPropertyValueChanged(object sender, PropertyChangedEventArgs e)
        {
            OnDataChanged();

            if (string.Equals(nameof(MaterialConnectionProperties.Method), e.PropertyName)) {
                //OnPropertyChanged(nameof(IsTypeMultiPart));
                //OnPropertyChanged(nameof(IsTypeCTM));
                OnPropertyChanged(nameof(IsMethodNotNone));

                //SelectedPart = null;
                //UpdatePartsList();
            }
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

    //public class MultiPartPropertyCollection : PropertyCollectionBase<MaterialPart>
    //{
    //    public MultiPartPropertyCollection()
    //    {
    //        AddText<string>("Name", nameof(MaterialPart.Name));
    //        AddText<int?>("Left", nameof(MaterialPart.Left), 0);
    //        AddText<int?>("Top", nameof(MaterialPart.Top), 0);
    //        AddText<int?>("Width", nameof(MaterialPart.Width), 0);
    //        AddText<int?>("Height", nameof(MaterialPart.Height), 0);
    //    }
    //}

    public class CTMPropertyCollection : PropertyCollectionBase<MaterialConnectionProperties>
    {
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> widthRow;
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> heightRow;
        private readonly IEditTextPropertyRow<MaterialConnectionProperties> matchBlocksRow;


        public CTMPropertyCollection()
        {
            widthRow = AddText("Width", nameof(MaterialConnectionProperties.Width), 1);
            heightRow = AddText("Height", nameof(MaterialConnectionProperties.Height), 1);
            matchBlocksRow = AddText<string>("Match Blocks", nameof(MaterialConnectionProperties.MatchBlocks));
            AddText<string>("Match Tiles", nameof(MaterialConnectionProperties.MatchTiles));
            
            AddBool("Placeholder", nameof(MaterialConnectionProperties.Placeholder), false);
        }

        public void SetData(MaterialProperties material)
        {
            base.SetData(material?.CTM);

            var hasMatchTiles = !string.IsNullOrWhiteSpace(material?.CTM?.MatchTiles);
            matchBlocksRow.DefaultValue = !hasMatchTiles ? material?.Name : null;

            var bounds = material?.CTM != null ? CtmTypes.GetBounds(material.CTM) : null;
            widthRow.DefaultValue = bounds?.Width ?? 1;
            heightRow.DefaultValue = bounds?.Height ?? 1;

            var isFixedSize = CtmTypes.IsFixedSize(material?.CTM?.Method);
            widthRow.Enabled = heightRow.Enabled = !isFixedSize;
        }
    }
}
