using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Prediction.Api;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.Model
{
    public abstract class PredictionItem : NotifyPropertyChangedBase
    {
        internal readonly KeyboardHost host;

        internal PredictionItem(KeyboardHost host, int index)
        {
            this.host = host;

            Index = index;
            Accept = new RelayCommand(AcceptAction);
        }

        public int Index { get; private set; }

        public bool IsEnabled { get { return isEnabledField; } set { SetProperty(ref isEnabledField, value); } }
        bool isEnabledField;

        public Visibility Visibility { get { return visibilityField; } set { SetProperty(ref visibilityField, value); } }
        Visibility visibilityField = Visibility.Collapsed;

        public string Keytop { get { return keytopField; } set { SetProperty(ref keytopField, value); } }
        string keytopField;

        public ICommand Accept { get; private set; }

        internal IPredictionSuggestion Suggestion { get; set; }

        protected abstract void AcceptAction(object o);
    }
}
