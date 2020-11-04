using PixelGraph.Common;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace PixelGraph.UI.Internal
{
    [CategoryOrder("Metadata", 1)]
    [CategoryOrder("Input", 2)]
    [CategoryOrder("Output", 3)]
    internal class PackPropertiesEditor
    {
        private readonly PackProperties pack;


        public PackPropertiesEditor(PackProperties pack = null)
        {
            this.pack = pack ?? new PackProperties();
        }

        [Editor]
        [PropertyOrder(0)]
        [Category("Metadata")]
        [DisplayName("Edition")]
        [ItemsSource(typeof(PackEditionItemsSource))]
        [Description("The edition of minecraft that will be using the resource pack.")]
        public string PackEdition {
            get => pack.PackEdition;
            set => SetProperty("pack.edition", value);
        }

        [Editor]
        [PropertyOrder(1)]
        [Category("Metadata")]
        [DisplayName("Format")]
        public int PackFormat {
            get => pack.PackFormat;
            set => SetProperty("pack.format", value);
        }

        [Editor]
        [PropertyOrder(2)]
        [Category("Metadata")]
        [DisplayName("Description")]
        public string PackDescription {
            get => pack.PackDescription;
            set => SetProperty("pack.description", value);
        }

        [Editor]
        [PropertyOrder(3)]
        [Category("Metadata")]
        [DisplayName("Tags")]
        public string PackTags {
            get => pack.PackTags;
            set => SetProperty("pack.tags", value);
        }

        [Editor]
        [PropertyOrder(0)]
        [Category("Input")]
        [DisplayName("Format")]
        [ItemsSource(typeof(FormatItemsSource))]
        public string InputFormat {
            get => pack.InputFormat;
            set => SetProperty("input.format", value);
        }

        [Editor]
        [PropertyOrder(0)]
        [Category("Output")]
        [DisplayName("Format")]
        [ItemsSource(typeof(FormatItemsSource))]
        public string OutputFormat {
            get => pack.OutputFormat;
            set => SetProperty("output.format", value);
        }

        private void SetProperty(string propertyName, object value)
        {
            pack.Properties[propertyName] = value as string ?? value?.ToString();
            // TODO: notify
        }
    }

    public class PackEditionItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            return new ItemCollection {
                {"java", "Java"},
                {"bedrock", "Bedrock"},
            };
        }
    }

    public class FormatItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            return new ItemCollection {
                {"", "None"},
                {"default", "Default"},
                {"lab-1.1", "Lab 1.1"},
                {"lab-1.3", "Lab 1.3"},
            };
        }
    }
}
