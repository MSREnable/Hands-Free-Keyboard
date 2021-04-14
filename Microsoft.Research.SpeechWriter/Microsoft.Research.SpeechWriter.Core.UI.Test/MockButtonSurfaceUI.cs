using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.UI.Test
{
    class MockButtonSurfaceUI : IButtonSurfaceUI<MockButton>
    {
        private int _nextButtonId;

        private readonly Dictionary<int, MockButton> _buttons = new Dictionary<int, MockButton>();

        internal double ButtonSize => 8;

        internal double ButtonMargin => 2;

        internal int GridWidth { get; private set; }

        internal int GridHeight { get; private set; }

        internal double Width => GridWidth * (ButtonMargin + ButtonSize) + ButtonMargin;

        internal double Height => GridHeight * (ButtonMargin + ButtonSize) + ButtonMargin;

        double IButtonSurfaceUI<MockButton>.TotalWidth => Width;

        double IButtonSurfaceUI<MockButton>.TotalHeight => Height;

        event EventHandler IButtonSurfaceUI<MockButton>.Resized
        {
            add
            {
                _resized += value;
            }

            remove
            {
                _resized -= value;
            }
        }
        private EventHandler _resized;

        internal void RaiseResize(int columns, int rows)
        {
            GridWidth = columns;
            GridHeight = rows;
            _resized?.Invoke(this, EventArgs.Empty);
        }

        MockButton IButtonSurfaceUI<MockButton>.Create(ITile tile, double width, double height, WidthBehavior behavior)
        {
            var button = new MockButton(_nextButtonId, tile, width, height, behavior);
            _buttons.Add(_nextButtonId, button);
            _nextButtonId++;

            return button;
        }

        void IButtonSurfaceUI<MockButton>.Move(MockButton element, double x, double y)
        {
            Assert.AreSame(element, _buttons[element.Id]);

            element.X = x;
            element.Y = y;
        }

        void IButtonSurfaceUI<MockButton>.Remove(MockButton element)
        {
            Assert.AreSame(element, _buttons[element.Id]);

            _buttons.Remove(element.Id);
        }

        internal MockButton FindButtonByGridCenter(double column, double row)
        {
            var x = column * (ButtonSize + ButtonMargin) + ButtonMargin + ButtonSize / 2;
            var y = row * (ButtonSize + ButtonMargin) + ButtonMargin + ButtonSize / 2;

            MockButton found = null;
            foreach (var button in _buttons.Values)
            {
                if (button.X <= x && x <= button.X + button.Width &&
                    button.Y <= y && y <= button.Y + button.Height)
                {
                    Assert.IsNull(found);
                    found = button;
                }
            }

            Assert.IsNotNull(found);

            return found;
        }

        internal void InvokeButtonByGrid(double column, double row)
        {
            var button = FindButtonByGridCenter(column, row);
            button.Invoke();
        }
    }
}
