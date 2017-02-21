namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Blank space.
    /// </summary>
    public class GapKeyLayout : IndividualKeyLayout
    {
        internal override void Layout(ILayoutContext context, double left, double top, double width, double height)
        {
            context.CreateGapKey(this, left, top, width, height);
        }
    }
}
