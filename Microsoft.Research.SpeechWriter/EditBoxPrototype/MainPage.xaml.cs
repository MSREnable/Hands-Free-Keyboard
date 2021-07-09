using Microsoft.Research.SpeechWriter.Core.Data;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

            var sequence = TileSequence.FromRaw(text);

            ItemsContainer.Items.Clear();
            foreach (var child in sequence)
            {
                ItemsContainer.Items.Add(new TileVisualizationElement(child.Type, child.Content, TileColor.Text, TileColor.HeadBackground));
            }
        }

        private void OnCursedTextChanged(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            var text = box.Text;

            var sequence = TileSequence.FromRaw(text);

            var start = box.SelectionStart;
            var length = box.SelectionLength;

            Debug.WriteLine($"Selection is {start} for {length}");

            var list = new List<TileVisualizationElement>();

            var charPosition = 0;
            var tilePosition = 0;
            while (tilePosition < sequence.Count &&
                charPosition + sequence[tilePosition].Content.Length <= start)
            {
                list.Add(new TileVisualizationElement(sequence[tilePosition].Type, sequence[tilePosition].Content, TileColor.Text, TileColor.HeadBackground));

                charPosition += sequence[tilePosition].Content.Length;
                tilePosition++;
                if (tilePosition < sequence.Count &&
                    !sequence[tilePosition - 1].IsPrefix &&
                    !sequence[tilePosition].IsSuffix)
                {
                    charPosition++;
                }
            }

            if (length == 0)
            {
                if (tilePosition < sequence.Count)
                {
                    if (charPosition < start)
                    {
                        var tile = sequence[tilePosition];
                        tilePosition++;

                        var prefix = TileData.Create(tile.Content.Substring(0, start - charPosition),
                            isPrefix: true,
                            isSuffix: tile.IsSuffix);
                        list.Add(new TileVisualizationElement(prefix.Type, prefix.Content, TileColor.Text, TileColor.HeadBackground));

                        var caret = TileData.Create("^");
                        list.Add(new TileVisualizationElement(caret.Type, caret.Content, TileColor.Text, TileColor.SuggestionPartBackground));

                        var suffix = TileData.Create(tile.Content.Substring(start - charPosition),
                            isPrefix: tile.IsPrefix,
                            isSuffix: true);
                        list.Add(new TileVisualizationElement(suffix.Type, suffix.Content, TileColor.Text, TileColor.HeadBackground));
                    }
                    else
                    {
                        var caret = TileData.Create("^",
                            isPrefix: start == charPosition,
                            isSuffix: start < charPosition);
                        list.Add(new TileVisualizationElement(caret.Type, caret.Content, TileColor.Text, TileColor.SuggestionPartBackground));
                    }
                }
                else
                {
                    var caret = TileData.Create("^", isSuffix: true);
                    list.Add(new TileVisualizationElement(caret.Type, caret.Content, TileColor.Text, TileColor.SuggestionPartBackground));
                }
            }
            else
            {

            }

            while (tilePosition < sequence.Count)
            {
                list.Add(new TileVisualizationElement(sequence[tilePosition].Type, sequence[tilePosition].Content, TileColor.Text, TileColor.HeadBackground));
                tilePosition++;
            }

            CursedContainer.Items.Clear();
            foreach (var child in list)
            {
                CursedContainer.Items.Add(child);
            }
        }
    }
}
