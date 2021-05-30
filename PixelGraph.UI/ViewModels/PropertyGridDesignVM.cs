namespace PixelGraph.UI.ViewModels
{
    public class PropertyGridDesignVM : ViewModelBase
    {
        public SampleData Data {get; set;}
        public SampleCollection DataModel {get; set;}


        public PropertyGridDesignVM()
        {
            Data = new SampleData {
                RowA = "Hello!",
            };

            DataModel = new SampleCollection();
            DataModel.SetData(Data);
        }

        public class SampleCollection : PropertyCollectionBase<SampleData>
        {
            public SampleCollection()
            {
                Add("Row A", nameof(SampleData.RowA), "Default A");
                Add("Row B", nameof(SampleData.RowB), "Default B");
                Add("Row C", nameof(SampleData.RowC), "Default C");
            }
        }

        public class SampleData
        {
            public string RowA {get; set;}
            public string RowB {get; set;}
            public string RowC {get; set;}
        }
    }
}
