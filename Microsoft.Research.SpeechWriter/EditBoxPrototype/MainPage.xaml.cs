using Microsoft.Research.SpeechWriter.Core.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EditBoxPrototype
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void OnTextChanged(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            var text = box.Text;

            Block.Text = text;

            var sequence = TileSequence.FromRaw(text);

            ItemsContainer.Items.Clear();
            foreach (var child in sequence)
            {
                ItemsContainer.Items.Add(child);
            }
        }
    }
}
