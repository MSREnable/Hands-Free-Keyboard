using System.Collections.Generic;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Individual key or cluster of keys.
    /// </summary>
    public abstract class KeyLayout
    {
        internal virtual void AssertValid(IKeyboardHost host)
        {
        }

        internal virtual void GatherKeyboardStates(ISet<string> states)
        {
        }

        internal abstract double CalculateWidth();

        internal abstract double CalculateTopOffset();

        internal abstract void Layout(ILayoutContext context, double left, double top, double width, double height);
    }
}
