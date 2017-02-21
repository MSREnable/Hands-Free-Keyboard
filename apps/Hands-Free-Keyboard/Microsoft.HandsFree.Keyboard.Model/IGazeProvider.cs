using Microsoft.HandsFree.Prediction.Api;
using System.Windows;

namespace Microsoft.HandsFree.Keyboard.Model
{
    /// <summary>
    /// Gaze system provider
    /// </summary>
    public interface IGazeProvider
    {
        /// <summary>
        /// Obtain "mean" value for gaze signal.
        /// </summary>
        Point SignalMean { get; }

        /// <summary>
        /// Obtain "standard deviation" value for gaze signel.
        /// </summary>
        Point SignalStandardDeviation { get; }

        /// <summary>
        /// Reset statistics gathering f
        /// </summary>
        void Reset();

        /// <summary>
        /// Launch the recallibration user interface.
        /// </summary>
        void LaunchRecalibration();

        /// <summary>
        /// Set character gaze timings based on prediction suggestions.
        /// </summary>
        /// <param name="characterSuggestions">The character prediction suggestions.</param>
        void SetCharacterSuggestions(IPredictionSuggestionCollection characterSuggestions);
    }
}
