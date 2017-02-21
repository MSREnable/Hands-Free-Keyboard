using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    /// <summary>
    /// Collection of conditional key layouts.
    /// </summary>
    public class ConditionalGroupLayout : KeyLayout
    {
        /// <summary>
        /// The conditional layouts.
        /// </summary>
        [XmlElement("Conditional")]
        public ConditionalLayout[] Conditionals { get; set; }

        internal override void AssertValid(IKeyboardHost host)
        {
            base.AssertValid(host);

            KeyboardValidationException.Assert(Conditionals != null, "Conditionals must be specified");
            KeyboardValidationException.Assert(Conditionals.Length != 0, "Conditionals cannot be empty");

            var isFirst = true;
            var hasDefault = false;
            var width = Conditionals[0].CalculateWidth();

            var names = new HashSet<string>();

            foreach (var conditional in Conditionals)
            {
                conditional.AssertValid(host);

                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    var otherWidth = conditional.CalculateWidth();
                    KeyboardValidationException.Assert(width == otherWidth, "All Conditionals must have same width");
                }

                if (conditional.Name == null)
                {
                    KeyboardValidationException.Assert(!hasDefault, "Can only have one default conditional");
                    hasDefault = true;
                }
                else
                {
                    KeyboardValidationException.Assert(!names.Contains(conditional.Name), "Names must be unique");
                    names.Add(conditional.Name);
                }
            }

            KeyboardValidationException.Assert(hasDefault, "Must have an unnamed default conditional");
        }

        internal override void GatherKeyboardStates(ISet<string> states)
        {
            base.GatherKeyboardStates(states);

            foreach (var conditional in Conditionals)
            {
                var name = conditional.Name;
                if (name != null)
                {
                    states.Add(name);
                }

                conditional.GatherKeyboardStates(states);
            }
        }

        internal override double CalculateWidth()
        {
            var width = Conditionals[0].CalculateWidth();
            return width;
        }

        internal override double CalculateTopOffset()
        {
            return 0;
        }

        ConditionalLayout GetCurrentLayout(string name)
        {
            ConditionalLayout selectedLayout = null;
            ConditionalLayout defaultLayout = null;

            var enumerator = Conditionals.GetEnumerator();
            while (selectedLayout == null && enumerator.MoveNext())
            {
                var conditional = (ConditionalLayout)enumerator.Current;

                if (conditional.Name == name)
                {
                    selectedLayout = conditional;
                }
                else if (conditional.Name == null)
                {
                    defaultLayout = conditional;
                }
            }

            return selectedLayout ?? defaultLayout;
        }

        internal override void Layout(ILayoutContext context, double left, double top, double width, double height)
        {
            // Collect together all the states.
            var availableStates = new HashSet<string>();
            foreach (var conditional in Conditionals)
            {
                var state = conditional.Name;
                if (state != null)
                {
                    availableStates.Add(state);
                }
            }

            foreach (var conditional in Conditionals)
            {
                if (conditional.Name == null)
                {
                    context.SetDefaultBinding(availableStates);
                }
                else
                {
                    context.SetNamedBinding(conditional.Name);
                }

                var x = left;
                foreach (var key in conditional.Keys)
                {
                    var keyWidth = context.KeySize * key.CalculateWidth();
                    var y = top + context.KeySize * key.CalculateTopOffset();

                    key.Layout(context, x, y, keyWidth, height);

                    x += keyWidth;
                }

                context.ResetBinding();
            }
        }
    }
}
