using Microsoft.HandsFree.Keyboard.Controls.Layout;
using Microsoft.HandsFree.Keyboard.UserInterface;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.Model.Test
{
    [TestClass]
    public class KeyboardBehaviorTest
    {
        readonly SortedSet<string> _keystrokes = new SortedSet<string>();

        void Walk(KeyLayout key)
        {
            var character = key as CharacterKeyLayout;
            if (character != null)
            {
                var unshifted = character.Value ?? character.Caption;
                _keystrokes.Add(KeyboardHost.AutoEscape(unshifted));
                KeystrokeCasesTest.CheckCovered(unshifted);

                var shifted = character.ShiftValue ?? character.ShiftCaption ?? (unshifted.ToUpperInvariant() != unshifted ? unshifted.ToUpperInvariant() : unshifted);
                _keystrokes.Add(KeyboardHost.AutoEscape(shifted));
                KeystrokeCasesTest.CheckCovered(shifted);

                switch (unshifted)
                {
                    case "{HOME}":
                    case "{END}":
                    case "{LEFT}":
                    case "{RIGHT}":
                        Assert.AreEqual("+" + unshifted, shifted, "Special case shifts");
                        break;
                }
            }
            else
            {
                var group = key as ConditionalGroupLayout;
                if (group != null)
                {
                    foreach (var conditional in group.Conditionals)
                    {
                        foreach (var childKey in conditional.Keys)
                        {
                            Walk(childKey);
                        }
                    }
                }
            }
        }

        void Walk(KeyboardRowLayout layout)
        {
            foreach (var key in layout.Keys)
            {
                Walk(key);
            }
        }

        void Walk(KeyboardLayout layout)
        {
            //layout.AssertValid(null);

            foreach (var row in layout.Rows)
            {
                Walk(row);
            }
        }

        [TestMethod]
        public void GatherKeystrokes()
        {
            _keystrokes.Clear();

            foreach (KeyboardLayoutName name in Enum.GetValues(typeof(KeyboardLayoutName)))
            {
                var xml = KeyboardPanel.GetKeyboardLayoutXml(name);
                if (xml != null)
                {
                    var layout = KeyboardLayout.Load(xml);

                    Walk(layout);
                }
            }

            KeystrokeCasesTest.AssertEverything(_keystrokes);
        }
    }
}
