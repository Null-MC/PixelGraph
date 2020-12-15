using System.Windows.Controls;
using System.Windows.Data;

namespace PixelGraph.UI.Controls
{
    internal class DataGridComboBoxExColumn2 : DataGridComboBoxColumn
    {
        private BindingBase _placeholderBinding;

        public virtual BindingBase PlaceholderBinding {
            get => _placeholderBinding;
            set {
                if (_placeholderBinding == value) return;
                var oldBinding = _placeholderBinding;
                _placeholderBinding = value;
                CoerceValue(IsReadOnlyProperty);
                CoerceValue(SortMemberPathProperty);
                OnPlaceholderBindingChanged(oldBinding, _placeholderBinding);
            }
        }


        protected virtual void OnPlaceholderBindingChanged(BindingBase oldBinding, BindingBase newBinding)
        {
            NotifyPropertyChanged(nameof(PlaceholderBinding));
        }
    }
}
