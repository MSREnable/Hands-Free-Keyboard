using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings.Nudgers;
using Microsoft.HandsFree.Settings.Serialization;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.HandsFree.Keyboard.Settings
{
    /// <summary>
    /// Prediction settings.
    /// </summary>
    public class PredictionSettings : NotifyingObject
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public PredictionSettings()
        {
            PredictorNudger = new DynamicValueNudger(this, nameof(Predictor), "Predictor", "Unknown: {0}");
        }

        /// <summary>
        /// The name of the predictor.
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Predictor", SettingNudgerFactoryBehavior.NameAssociatedMember)]
        public string Predictor { get { return _predictor; } set { SetProperty(ref _predictor, value); } }
        string _predictor = "Default";

        /// <summary>
        /// The prediction layout to display.
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Predictions layout")]
        public PredictionLayout PredictionLayout { get { return _predictionLayout; } set { SetProperty(ref _predictionLayout, value); } }
        PredictionLayout _predictionLayout = PredictionLayout.WordsAlone;

        /// <summary>
        /// The nudger for the predictor.
        /// </summary>
        [XmlIgnore]
        public IDynamicValueNudger PredictorNudger { get; private set; }

        /// <summary>
        /// How many keytop hints should be shown?
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Key Top Hints", 0, 1)]
        public int KeyTopHints { get { return _keyTopHints; } set { SetProperty(ref _keyTopHints, value); } }
        int _keyTopHints = 1;

        /// <summary>
        /// How long should the top ranked prediction be displayed on keytops.
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Hint display time (ms, 0 for no interval)", 0, 10000, 250)]
        public int KeyTopHintInterval { get { return _keyTopHintInterval; } set { SetProperty(ref _keyTopHintInterval, value); } }
        int _keyTopHintInterval = 1000;

        /// <summary>
        /// How should novelty be applied to the top ranked word prediction?
        /// </summary>
        [XmlAttribute]
        [SettingDescription("First suggestion novelty")]
        public PredictionNovelty PredictionNovelty { get { return _predictionNovelty; } set { SetProperty(ref _predictionNovelty, value); } }
        PredictionNovelty _predictionNovelty = PredictionNovelty.FromFirstLetter;

        /// <summary>
        /// How should spacing be managed after insertion of a prediction.
        /// </summary>
        [XmlAttribute]
        [SettingDescription("Auto space behavior")]
        public PredictionSpacingBehavior PredictionSpacingBehavior { get { return _predictionSpacingBehavior; } set { SetProperty(ref _predictionSpacingBehavior, value); } }
        PredictionSpacingBehavior _predictionSpacingBehavior;

        /// <summary>
        /// Should activation delays be predicted?
        /// </summary>
        [XmlAttribute]
        [DefaultValue(true)]
        [SettingDescription("Predict button pushes")]
        public bool PredictCharacters { get { return _predictCharacters; } set { SetProperty(ref _predictCharacters, value); } }
        bool _predictCharacters = true;
    }
}
