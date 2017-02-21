using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Nudgers;
using Microsoft.HandsFree.Settings.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.HandsFree.Settings.Test
{
    [TestClass]
    public class ValueNudgerTest
    {
        class Target<T> : NotifyingObject
        {
            internal readonly IValueNudger _nudger;

            internal Target(Func<Target<T>, IValueNudger> getNudger)
            {
                _nudger = getNudger(this);
            }

            internal Target(Action<ISettingsSerializationContext, string> doSerialization)
            {
                var context = new NudgerFinder(this, nameof(Value));
                doSerialization(context, nameof(Value));
                _nudger = context.Nudger;
            }

            [SettingDescription("The value")]
            public T Value { get { return _value; } set { SetProperty(ref _value, value); } }
            T _value;

            void Nudge(ICommand command, T[] expecteds)
            {
                foreach (var expected in expecteds)
                {
                    Assert.IsTrue(command.CanExecute(null), "Can perform nudge");
                    command.Execute(null);
                    Assert.AreEqual(expected, Value, "Nudged to expected value");
                }
            }

            void NudgeEnd(ICommand command, T[] expecteds)
            {
                Nudge(command, expecteds);
                Assert.IsFalse(command.CanExecute(null), "Reached nudge limit");
            }

            internal void NudgeUp(params T[] expecteds)
            {
                Nudge(_nudger.NudgeUp, expecteds);
            }

            internal void NudgeUpEnd(params T[] expecteds)
            {
                NudgeEnd(_nudger.NudgeUp, expecteds);
            }

            internal void NudgeDown(params T[] expecteds)
            {
                Nudge(_nudger.NudgeDown, expecteds);
            }

            internal void NudgeDownEnd(params T[] expecteds)
            {
                NudgeEnd(_nudger.NudgeDown, expecteds);
            }
        }

        enum Enumeration
        {
            One,

            Two,

            [System.ComponentModel.Description("Thrice")]
            Three,

            Four
        }

        class ActualDynamicValueSetting : DynamicValueSetting
        {

        }

        static void BasicVisibility<T>(Target<T> target)
        {
            if (target.Value is bool)
            {
                Assert.AreEqual(Visibility.Collapsed, target._nudger.UpDownInterfaceVisibility);
                Assert.AreEqual(Visibility.Visible, target._nudger.BooleanInterfaceVisibility);
            }
            else
            {
                Assert.AreEqual(Visibility.Visible, target._nudger.UpDownInterfaceVisibility);
                Assert.AreEqual(Visibility.Collapsed, target._nudger.BooleanInterfaceVisibility);
            }
        }

        [TestMethod]
        public void BooleanValueNudgerTest()
        {
            var target = new Target<bool>((t) => new BooleanValueNudger(t, nameof(t.Value), "Boolean"));
            BasicVisibility(target);

            target.NudgeUp(true, true);
            target.NudgeDown(false, false);
        }

        //[TestMethod]
        //public void BooleanValueNudgerTestEx()
        //{
        //    var target = new Target<bool>((c, n) => { c.Serialize(n, false, "Boolean"); });
        //    BasicVisibility(target);

        //    target.NudgeUp(true, true);
        //    target.NudgeDown(false, false);

        //    var nudger = (ValueNudger)target._nudger;
        //    Assert.IsFalse(target.Value);
        //    Assert.IsFalse(nudger.ValueBool);

        //    nudger.ValueBool = true;
        //    Assert.IsTrue(target.Value);
        //    Assert.IsTrue(nudger.ValueBool);
        //}

        //[TestMethod]
        //public void DoubleValueNudgerTest()
        //{
        //    var target = new Target<double>((c, n) => { c.Serialize(n, 1.0, "Double", 0, 3, 1); }) { Value = 1 };
        //    BasicVisibility(target);

        //    target.NudgeUpEnd(2, 3);
        //    target.NudgeDownEnd(2, 1, 0);
        //    target.NudgeUp(1);
        //}

        //[TestMethod]
        //public void IntValueNudgerTest()
        //{
        //    var target = new Target<int>((c, n) => { c.Serialize(n, 2, "Int", 0, 6, 2); }) { Value = 2 };
        //    BasicVisibility(target);

        //    target.NudgeUpEnd(4, 6);
        //    target.NudgeDownEnd(4, 2, 0);
        //    target.NudgeUp(2);
        //}

        //[TestMethod]
        //public void EnumValueNudgerTest()
        //{
        //    var target = new Target<Enumeration>((c, n) => { c.Serialize(n, Enumeration.Two, "Enum"); }) { Value = Enumeration.Two };
        //    BasicVisibility(target);

        //    target.NudgeUp(Enumeration.Three, Enumeration.Four, Enumeration.One, Enumeration.Two, Enumeration.Three, Enumeration.Four);
        //    target.NudgeDown(Enumeration.Three, Enumeration.Two, Enumeration.One, Enumeration.Four, Enumeration.Three);
        //    Assert.AreEqual("Thrice", target._nudger.ValueString);

        //    target.NudgeUp(Enumeration.Four, Enumeration.One);
        //    Assert.AreEqual("One", target._nudger.ValueString);
        //}

        [TestMethod]
        public void DynamicValueNudgerTest()
        {
            var target = new Target<string>((t) => new DynamicValueNudger(t, nameof(t.Value), "Dynamic", "Unknown")) { Value = "B" };
            var nudger = (DynamicValueNudger)target._nudger;
            nudger.Values = new DynamicValueSetting[]
                {
                    new ActualDynamicValueSetting {Key="A", ValueString="Alpha" },
                    new ActualDynamicValueSetting {Key="B", ValueString="Beta" },
                    new ActualDynamicValueSetting {Key="C", ValueString="Gamma" },
                };
            BasicVisibility(target);

            target.NudgeUp("C", "A", "B", "C", "A");
            target.NudgeDown("C", "B", "A", "C");

            Assert.AreEqual("Gamma", target._nudger.ValueString);

            target.Value = "Omega";
            Assert.AreEqual("Unknown", target._nudger.ValueString);
        }

        //[TestMethod]
        //public void NotifiesFireTest()
        //{
        //    var target = new Target<double>((c, n) => { c.Serialize(n, 1.0, "Double", 0, 3, 1); }) { Value = 1 };
        //    BasicVisibility(target);

        //    var count = 0;
        //    target._nudger.PropertyChanged += (s, e) => count++;

        //    Assert.AreEqual(0, count);
        //    target.Value = 0;
        //    Assert.AreEqual(3, count);
        //}

        //[TestMethod]
        //public void BitsAndBobsTest()
        //{
        //    var target = new Target<double>((c, n) => { c.Serialize(n, 1.0, "Double", 0, 3, 1); }) { Value = 1 };
        //    BasicVisibility(target);

        //    var nudger = (ValueNudger)target._nudger;
        //    Assert.IsFalse(nudger.ValueBool);
        //    nudger.ValueBool = true;
        //    Assert.AreEqual("1", nudger.ValueString);
        //    Assert.AreEqual("Double", nudger.Description);
        //}
    }
}
