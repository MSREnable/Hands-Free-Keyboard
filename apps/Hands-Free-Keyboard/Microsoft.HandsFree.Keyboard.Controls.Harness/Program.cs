using Microsoft.HandsFree.Keyboard.ConcreteImplementations;
using Microsoft.HandsFree.Keyboard.Controls.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace Microsoft.HandsFree.Keyboard.Controls.Harness
{
    class Program
    {
        static void SerializeToConsole(KeyboardLayout keyboard)
        {
            var stream = new MemoryStream();
            var writer = new XmlTextWriter(stream, Encoding.UTF8)
            {
                Formatting = Formatting.Indented
            };
            KeyboardLayout.Serializer.Serialize(writer, keyboard);
            var bytes = stream.ToArray();
            var text = Encoding.UTF8.GetString(bytes);
            Console.Write(text);

            File.WriteAllBytes("Keyboard.xml", bytes);
        }

        [STAThread]
        static void Main(string[] args)
        {
            var keyboard = new KeyboardLayout
            {
                Rows = new KeyboardRowLayout[]
                {
                    new KeyboardRowLayout
                    {
                        Keys = new KeyLayout[]
                        {
                            new ActionKeyLayout { Caption = "esc", Action = "Esc_Button_Click" },
                            new ConditionalGroupLayout
                            {
                                Conditionals = new ConditionalLayout[]
                                {
                                    new ConditionalLayout
                                    {
                                        Keys = new KeyLayout[]
                                        {
                                            new CharacterKeyLayout { Caption = "1", ShiftCaption = "!" },
                                            new CharacterKeyLayout { Caption = "2", ShiftCaption = "@" },
                                            new CharacterKeyLayout { Caption = "3", ShiftCaption = "#" },
                                            new CharacterKeyLayout { Caption = "4", ShiftCaption = "$" },
                                            new CharacterKeyLayout { Caption = "5", ShiftCaption = "%" },
                                            new CharacterKeyLayout { Caption = "6", ShiftCaption = "^" },
                                            new CharacterKeyLayout { Caption = "7", ShiftCaption = "&" },
                                            new CharacterKeyLayout { Caption = "8", ShiftCaption = "*" },
                                            new CharacterKeyLayout { Caption = "9", ShiftCaption = "(" },
                                            new CharacterKeyLayout { Caption = "0", ShiftCaption = ")" },
                                            new StateKeyLayout { Caption = "&123", KeyWidth = 2, StateName = "Numeric" }
                                        }
                                    },
                                    new ConditionalLayout
                                    {
                                        Name = "Numeric",
                                        Keys = new KeyLayout[]
                                        {
                                            new ActionKeyLayout { Caption = "?", FontSize = 30, Action = "Left_Button_Click" },
                                            new GapKeyLayout { KeyWidth = 2 },
                                            new ActionKeyLayout { Caption = "?", FontSize = 30, Action = "Right_Button_Click" },
                                            new GapKeyLayout(),
                                            new CharacterKeyLayout { Caption = "," },
                                            new CharacterKeyLayout { Caption = "." },
                                            new GapKeyLayout { KeyWidth = 3 },
                                            new StateKeyLayout { Caption = "&123", KeyWidth = 2 }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    new KeyboardRowLayout
                    {
                        Keys = new KeyLayout[]
                        {
                            new ToggleKeyLayout { Caption = "ctrl", StateName="Control" },
                            new CharacterKeyLayout { Caption = "q" },
                            new CharacterKeyLayout { Caption = "w" },
                            new CharacterKeyLayout { Caption = "e" },
                            new CharacterKeyLayout { Caption = "r" },
                            new CharacterKeyLayout { Caption = "t" },
                            new CharacterKeyLayout { Caption = "y" },
                            new CharacterKeyLayout { Caption = "u" },
                            new CharacterKeyLayout { Caption = "i" },
                            new CharacterKeyLayout { Caption = "o" },
                            new CharacterKeyLayout { Caption = "p" },
                            new ActionKeyLayout { Caption = "\xE083", FontSize = 30, Action = "Backspace_Button_Click" },
                            new ActionKeyLayout { Caption = "del wrd", Action = "BackWord_Button_Click" }
                        }
                    },
                    new KeyboardRowLayout
                    {
                        Keys = new KeyLayout[]
                        {
                            new GapKeyLayout { KeyWidth = 0.25 },
                            new ToggleKeyLayout { Caption = "alt", StateName = "Alt" },
                            new CharacterKeyLayout { Caption = "a" },
                            new CharacterKeyLayout { Caption = "s" },
                            new CharacterKeyLayout { Caption = "d" },
                            new CharacterKeyLayout { Caption = "f" },
                            new CharacterKeyLayout { Caption = "g" },
                            new CharacterKeyLayout { Caption = "h" },
                            new CharacterKeyLayout { Caption = "j" },
                            new CharacterKeyLayout { Caption = "k" },
                            new CharacterKeyLayout { Caption = "l" },
                            new ActionKeyLayout { Caption = "\xE092", KeyWidth = 2.75, FontSize = 30, Action = "Return_Button_Click" }
                        }
                    },
                    new KeyboardRowLayout
                    {
                        Keys = new KeyLayout[]
                        {
                            new GapKeyLayout { KeyWidth = 0.75 },
                            new ToggleKeyLayout { Caption = "shift", StateName = "Shift" },
                            new CharacterKeyLayout { Caption = "z" },
                            new CharacterKeyLayout { Caption = "x" },
                            new CharacterKeyLayout { Caption = "c" },
                            new CharacterKeyLayout { Caption = "v" },
                            new CharacterKeyLayout { Caption = "b" },
                            new CharacterKeyLayout { Caption = "n" },
                            new CharacterKeyLayout { Caption = "m" },
                            new CharacterKeyLayout { Caption = "?" },
                            new ActionKeyLayout { Caption = "space", KeyWidth = 2, Action = "Space_Button_Click" },
                            new ToggleKeyLayout { Caption = "\xE089", KeyWidth = 1.25, FontSize = 30, StateName = "Fold" }
                        }
                    }
                }
            };

            var fakeHost = new FakeHost();
            keyboard.AssertValid(fakeHost);

            var mainWindow = new MainWindow();
            var environment = KeyboardApplicationEnvironment.Create(mainWindow);
            var realHost = environment.Host;
            keyboard.AssertValid(realHost);

            var states = new HashSet<string>();
            keyboard.GatherKeyboardStates(states);
            Console.WriteLine("Additional keyboard states are:");
            foreach (var state in states)
            {
                Console.WriteLine("  {0}", state);
            }

            SerializeToConsole(keyboard);

            Console.ReadLine();
        }
    }
}
