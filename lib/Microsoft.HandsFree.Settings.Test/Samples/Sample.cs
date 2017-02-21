using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Settings.Test.Samples
{
    class Sample : NotifyPropertyChangedBase
    {
        [SettingDescription]
        public string Name { get { return _name; } set { SetProperty(ref _name, value); } }
        string _name = "Default";

        [SettingDescription("The value", 0, 4, 2)]
        public int Value { get { return _value; } set { SetProperty(ref _value, value); } }
        int _value = 1;

        [SettingDescription("The factor", 0.0, 10.0, 0.5)]
        public double FiddleFactor { get { return _fiddleFactor; } set { SetProperty(ref _fiddleFactor, value); } }
        double _fiddleFactor = 3.14159;

        [SettingDescription("The problematic", 0, 100)]
        public int Problematic { get { return _problematic; } set { SetProperty(ref _problematic, value); } }
        int _problematic;

        public SampleChild Child { get { return _child; } set { SetProperty(ref _child, value); } }
        SampleChild _child;
    }
}
