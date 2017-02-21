using Microsoft.HandsFree.Settings.Serialization;

namespace Microsoft.HandsFree.Settings.Test.Samples
{
    class SampleChild : NotifyPropertyChangedBase
    {
        [SettingDescription("The payload", 0, 100)]
        public int Payload { get { return _payload; } set { SetProperty(ref _payload, value); } }
        int _payload = 42;
    }
}
