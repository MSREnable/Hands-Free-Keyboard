using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Mouse;
using Microsoft.HandsFree.Prediction.Api;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Implementation of IGazeProvider.
    /// </summary>
    public class GazeProvider : IGazeProvider
    {
        readonly GazeMouse _gazeMouse;

        readonly Dictionary<string, double> _sendKeysProbabilities = new Dictionary<string, double>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="window">The attaching window.</param>
        /// <param name="getGazeClickParameters">The gaze click parameter provider.</param>
        GazeProvider(Window window, GazeMouse.GetGazeClickParameters getGazeClickParameters)
        {
            _gazeMouse = GazeMouse.Attach(window, null, getGazeClickParameters ?? GetGazeClickParameters, AppSettings.Instance.Mouse);
        }

        /// <summary>
        /// Obtain "mean" value for gaze signal.
        /// </summary>
        public Point SignalMean => _gazeMouse.FilteredSignal.Mean;

        /// <summary>
        /// Obtain "standard deviation" value for gaze signel.
        /// </summary>
        public Point SignalStandardDeviation => _gazeMouse.FilteredSignal.StandardDeviation;

        /// <summary>
        /// Reset statistics gathering f
        /// </summary>
        public void Reset()
        {
            _gazeMouse.FilteredSignal.Reset();
            _gazeMouse.OriginalSignal.Reset();
        }

        /// <summary>
        /// Launch the recallibration user interface.
        /// </summary>
        public void LaunchRecalibration()
        {
            GazeMouse.LaunchRecalibration();
        }

        /// <summary>
        /// Create an instance of the provider for the given window.
        /// </summary>
        /// <param name="window">The window.</param>
        /// <returns>The provider.</returns>
        /// <param name="getGazeClickParameters">The gaze click parameter provider.</param>
        public static GazeProvider Create(Window window, GazeMouse.GetGazeClickParameters getGazeClickParameters)
        {
            var gazeProvider = new GazeProvider(window, getGazeClickParameters);
            return gazeProvider;
        }

        static string NormalizedSendKeys(string text)
        {
            var normalizedText = text != null && text.Length == 1 ? text.ToLowerInvariant() : text;
            return normalizedText;
        }

        GazeClickParameters GetGazeClickParameters(FrameworkElement element)
        {
            var clickParams = new GazeClickParameters
            {
                MouseDownDelay = GazeMouse.DefaultMouseDownDelay
            };

            // Look for an ancestor that implements IGazeTarget.
            var walker = element;
            var target = element as IGazeTarget;
            while (target == null && walker != null)
            {
                walker = VisualTreeHelper.GetParent(walker) as FrameworkElement;
                target = walker as IGazeTarget;
            }

            // If we don't have a IGazeTarget ancestor...
            if (target == null)
            {
                // ...use the default delays multiplied by attached properties (defaults are 1 and 0).
                var multiplier = Controls.HandsFree.GetMultiplier(element);
                var repeatMultiplier = Controls.HandsFree.GetRepeatMultiplier(element);

                clickParams.MouseUpDelay = (uint)(clickParams.MouseDownDelay +
                                            Math.Round(multiplier * (AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay)));
                clickParams.RepeatMouseDownDelay = double.IsNaN(repeatMultiplier) ?
                    uint.MaxValue :
                    (uint)(repeatMultiplier * (AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay));
            }
            else
            {
                // Get the keystrokes we're likely to send.
                var sendKeys = NormalizedSendKeys(target.SendKeys);

                // If we have keystrokes and they're known from the the predictor...
                double probability;
                if (sendKeys != null && _sendKeysProbabilities.TryGetValue(sendKeys, out probability))
                {
                    // ...use the probability to select the delay...
                    var delay = GetMouseUpDelayFromProbability(probability);
                    clickParams.MouseUpDelay = (uint)(clickParams.MouseDownDelay + Math.Round(target.Multiplier * delay));
                    clickParams.RepeatMouseDownDelay = 
                        AppSettings.Instance.Keyboard.IsNoAlphaAutorepeat ? uint.MaxValue : (uint)(clickParams.MouseUpDelay +
                                                        (AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay));
                }
                else
                {
                    // ...otherwise use the default delay.
                    var delay = AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay;
                    clickParams.MouseUpDelay = (uint)(clickParams.MouseDownDelay + Math.Round(target.Multiplier * delay));
                    clickParams.RepeatMouseDownDelay = double.IsNaN(target.RepeatMultiplier) ?
                        uint.MaxValue :
                        (uint)(clickParams.MouseUpDelay +
                                target.RepeatMultiplier * (AppSettings.Instance.Keyboard.GazeClickDelay - GazeMouse.DefaultMouseDownDelay));
                }
            }

            return clickParams;
        }

        int GetMouseUpDelayFromProbability(double probability)
        {
            var minMouseUpDelay = (KeyboardSettings.MinGazeClickDelay - (int)GazeMouse.DefaultMouseDownDelay);
            var maxMouseUpDelay = (AppSettings.Instance.Keyboard.GazeClickDelay - KeyboardSettings.MinGazeClickDelay);
            return minMouseUpDelay + (int)((1 - probability) * maxMouseUpDelay);
        }

        /// <summary>
        /// Set character gaze timings based on prediction suggestions.
        /// </summary>
        /// <param name="characterSuggestions">The character prediction suggestions.</param>
        public void SetCharacterSuggestions(IPredictionSuggestionCollection characterSuggestions)
        {
            _sendKeysProbabilities.Clear();
            foreach (var suggestion in characterSuggestions)
            {
                var normalizedText = NormalizedSendKeys(suggestion.Text);
                _sendKeysProbabilities.Add(normalizedText, suggestion.Confidence);
            }
        }
    }
}
