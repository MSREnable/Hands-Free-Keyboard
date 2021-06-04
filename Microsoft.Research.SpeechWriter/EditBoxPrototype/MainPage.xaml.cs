﻿using Microsoft.Research.SpeechWriter.Core.Data;
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
                ItemsContainer.Items.Add(child);
            }

            var start = box.SelectionStart;
            var length = box.SelectionLength;

            Debug.WriteLine($"Selection is {start} for {length}");

            var list = new List<TileData>();

            var charPosition = 0;
            var tilePosition = 0;
            while (tilePosition < sequence.Count &&
                charPosition + sequence[tilePosition].Content.Length <= start)
            {
                list.Add(sequence[tilePosition]);

                charPosition += sequence[tilePosition].Content.Length;
                tilePosition++;
                if (tilePosition < sequence.Count &&
                    !sequence[tilePosition - 1].IsGlueAfter &&
                    !sequence[tilePosition].IsGlueBefore)
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

                        var prefix = new TileData(tile.Content.Substring(0, start - charPosition),
                            isGlueBefore: tile.IsGlueBefore,
                            isGlueAfter: true);
                        list.Add(prefix);

                        var caret = new TileData("^");
                        list.Add(caret);

                        var suffix = new TileData(tile.Content.Substring(start - charPosition),
                            isGlueAfter: tile.IsGlueAfter, isGlueBefore: true);
                        list.Add(suffix);
                    }
                    else
                    {
                        var caret = new TileData("^",
                            isGlueBefore: start < charPosition,
                            isGlueAfter: start == charPosition);
                        list.Add(caret);
                    }
                }
                else
                {
                    var caret = new TileData("^", isGlueBefore: true);
                    list.Add(caret);
                }

            }
            else
            {

            }

            while (tilePosition < sequence.Count)
            {
                list.Add(sequence[tilePosition]);
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
