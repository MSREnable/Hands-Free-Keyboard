using Microsoft.Research.SpeechWriter.Core.Items;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.Core.Automation
{
    /// <summary>
    /// Class for finding actions to achieve a given goal within an instance of an ApplicationModel.
    /// </summary>
    public static class ApplicationRobot
    {
        /// <summary>
        /// Pick a random item to press.
        /// </summary>
        /// <param name="model">The model to act against.</param>
        /// <param name="random">The source of randomness.</param>
        /// <returns>A valid but random action to take.</returns>
        /// <remarks>Current implementations assumes child suggestions lists are finite.</remarks>
        public static ApplicationRobotAction GetRandom(ApplicationModel model, Random random)
        {
            ApplicationRobotAction action;

            var headCount = model.HeadItems.Count;
            var tailCount = model.TailItems.Count;
            var suggestionsCount = 0;
            foreach (var subList in model.SuggestionLists)
            {
                var subListCount = subList.Count();
                suggestionsCount += subListCount;
            }

            var index = random.Next(0, suggestionsCount);

            if (index < headCount)
            {
                action = ApplicationRobotAction.CreateHead(index);
            }
            else if (index < headCount + tailCount)
            {
                action = ApplicationRobotAction.CreateTail(index - headCount);
            }
            else
            {
                var subIndex = index - headCount - tailCount;
                index = 0;
                while (model.SuggestionLists[index].Count() <= subIndex)
                {
                    subIndex -= model.SuggestionLists[index].Count();
                    index++;
                }

                using (var enumerator = model.SuggestionLists[index].GetEnumerator())
                {
                    for (var position = 0; enumerator.MoveNext() && position < index; position++)
                    {
                    }

                    action = ApplicationRobotAction.CreateSuggestion(index, subIndex);
                }
            }

            return action;
        }

        private static bool IsItemMatch<T>(ITile tile, string value, CultureInfo culture)
        {
            return tile is T && StringEquals(tile.Content, value, culture);
        }

        private static ApplicationRobotAction CreateSuggestedWordAction(ApplicationModel model,
            bool complete,
            string[] words,
            int wordsMatchLim,
            int index,
            CultureInfo culture)
        {
            ApplicationRobotAction action;

            var list = model.SuggestionLists[index];

            using (var enumerator = list.GetEnumerator())
            {
                // Check the first word matches.
                enumerator.MoveNext();
                Debug.Assert(enumerator.Current is SuggestedWordItem);
                Debug.Assert(StringEquals(enumerator.Current.Content, words[wordsMatchLim], culture));

                // See if more words match.
                var subIndex = 0;
                while (enumerator.MoveNext() &&
                    wordsMatchLim + subIndex + 1 < words.Length &&
                    enumerator.Current is SuggestedWordItem &&
                    StringEquals(enumerator.Current.Content, words[wordsMatchLim + subIndex + 1], culture))
                {
                    subIndex++;
                }

                if (complete &&
                    wordsMatchLim + subIndex + 1 == words.Length &&
                    enumerator.Current is TailStopItem)
                {
                    // We can complete the action.
                    action = ApplicationRobotAction.CreateSuggestionAndComplete(index, subIndex + 1);
                }
                else
                {
                    // We can advance a word or more.
                    action = ApplicationRobotAction.CreateSuggestion(index, subIndex);
                }
            }

            return action;
        }

        private static bool StringEquals(string string1, string string2, CultureInfo culture)
        {
            var value = string.Compare(string1, string2, true, culture);
            return value == 0;
        }

        private static int StringCompare(string string1, string string2, CultureInfo culture)
        {
            return culture.CompareInfo.Compare(string1, string2, CompareOptions.StringSort);
        }

        private static bool StartsWith(string string1, string string2, CultureInfo culture)
        {
            // TODO: Would like to do this, but it doesn't work:
            // var cultureTruth = string1.StartsWith(string2, true, culture);

            // TODO: This also works, but may not be correct!
            // var ordinalTruth = string1.StartsWith(string2, StringComparison.OrdinalIgnoreCase);

            var string2Length = string2.Length;
            var value = string2Length <= string1.Length &&
                StringEquals(string1.Substring(0, string2Length), string2, culture);

            return value;
        }

        private static ApplicationRobotAction GetModeEscape(ApplicationModel model)
        {
            Debug.Assert(model.HeadItems[0] is HeadStartItem);

            var escapeLimit = 1;
            while (escapeLimit < model.HeadItems.Count &&
                model.HeadItems[escapeLimit] is HeadWordItem)
            {
                escapeLimit++;
            }

            var action = ApplicationRobotAction.CreateHead(escapeLimit - 1);
            return action;
        }

        private static ApplicationRobotAction GetSuggestionsAction(ApplicationModel model,
            bool complete,
            string[] words,
            int wordsMatchLim,
            CultureInfo culture)
        {
            ApplicationRobotAction action;

            var targetWord = words[wordsMatchLim];

            action = null;
            for (var index = 0; action == null && index < model.SuggestionLists.Count; index++)
            {
                var list = model.SuggestionLists[index];
                var firstItem = list.First();

                if (firstItem is SuggestedWordItem)
                {
                    var suggestedWord = firstItem.Content;

                    if (StringEquals(suggestedWord, targetWord, culture))
                    {
                        // Found our word.
                        action = CreateSuggestedWordAction(model, complete, words, wordsMatchLim, index, culture);
                    }
                    else if (StringCompare(targetWord, suggestedWord, culture) < 0)
                    {
                        // Need to step back.
                        action = ApplicationRobotAction.CreateInterstitial(index);
                    }
                }
                else if (firstItem is SuggestedSpellingItem)
                {
                    var partial = firstItem.Content;

                    if (StartsWith(targetWord, partial, culture))
                    {
                        var subIndex = 0;
                        using (var enumerator = list.GetEnumerator())
                        {
                            var more = enumerator.MoveNext();
                            Debug.Assert(more);
                            Debug.Assert(enumerator.Current is SuggestedSpellingItem);

                            more = enumerator.MoveNext();
                            while (more &&
                                enumerator.Current is SuggestedSpellingSequenceItem &&
                                StartsWith(targetWord, enumerator.Current.Content, culture))
                            {
                                subIndex++;
                                more = enumerator.MoveNext();
                            }

                            if (complete &&
                                more &&
                                enumerator.Current is SuggestedWordItem &&
                                StringEquals(enumerator.Current.Content, targetWord, culture))
                            {
                                action = ApplicationRobotAction.CreateSuggestion(index, subIndex + 1);
                            }
                            else
                            {
                                action = ApplicationRobotAction.CreateSuggestion(index, subIndex);
                            }
                        }
                    }
                    else if (!StartsWith(targetWord, partial.Substring(0, partial.Length - 1), culture))
                    {
                        // Need to remove incorrectly spelled items.
                        if (targetWord[0] != partial[0])
                        {
                            action = GetModeEscape(model);
                        }
                        else if (index != 0 && model.SuggestionLists[0].First() is SuggestedSpellingBackspaceItem)
                        {
                            action = ApplicationRobotAction.CreateSuggestion(0, 0);
                        }
                        else
                        {
                            // Need to erase unwanted letters, which means winding to top of spellings.
                            Debug.Assert(model.SuggestionLists[0].First() is InterstitialGapItem);
                            action = ApplicationRobotAction.CreateInterstitial(0);
                        }
                    }
                    else if (StringCompare(partial.Substring(partial.Length - 1, 1), targetWord.Substring(partial.Length - 1, 1), culture) < 0)
                    {
                        // Look at next item.
                    }
                    else
                    {
                        action = ApplicationRobotAction.CreateInterstitial(index);
                    }
                }
                else if (firstItem is SuggestedSpellingWordItem)
                {
                    if (StringEquals(firstItem.Content, targetWord, culture))
                    {
                        action = ApplicationRobotAction.CreateSuggestion(index, 0);
                    }
                }
                else if (firstItem is SuggestedSpellingBackspaceItem)
                {
                    if (!StartsWith(targetWord, firstItem.Content, culture))
                    {
                        action = ApplicationRobotAction.CreateSuggestion(index, 0);
                    }
                }
                else
                {
                    Debug.Assert(firstItem is SuggestedUnicodeItem);

                    var partial = firstItem.Content;

                    Debug.Assert(((SuggestedUnicodeItem)firstItem).Symbol.Length == 1);
                    Debug.Assert(((SuggestedUnicodeItem)firstItem).Symbol[0] == partial[partial.Length - 1]);

                    if (StartsWith(targetWord, partial, culture))
                    {
                        action = ApplicationRobotAction.CreateSuggestion(index, 0);
                    }
                    else if (!StartsWith(targetWord, partial.Substring(0, partial.Length - 1), culture))
                    {
                        action = GetModeEscape(model);
                    }
                    else if (partial[partial.Length - 1] < targetWord[partial.Length - 1])
                    {
                        // Can just move along.
                    }
                    else
                    {
                        // Need to step back.
                        Debug.Assert(model.SuggestionInterstitials[index] is InterstitialGapItem);
                        action = ApplicationRobotAction.CreateInterstitial(index);
                    }
                }
            }

            if (action == null)
            {
                // Need to step forward.
                action = ApplicationRobotAction.CreateInterstitial(model.SuggestionLists.Count);
            }

            return action;
        }

        /// <summary>
        /// Get the next action to achieve the given goal.
        /// </summary>
        /// <param name="model">The model to act against.</param>
        /// <param name="complete">Finish by presssing a Stop button and returning IsComplete true in the action, otherwise don't press Stop button and return null.</param>
        /// <param name="words">The words to be spoken.</param>
        /// <returns>The next action to take.</returns>
        private static ApplicationRobotAction FindNextAction(ApplicationModel model, bool complete, params string[] words)
        {
            ApplicationRobotAction action;

            Debug.Assert(model.HeadItems[0] is HeadStartItem);

            var culture = model.HeadItems[0].Culture;

            // Find number of words already correctly entered.
            var wordsMatchLim = 0;
            while (wordsMatchLim < words.Length &&
                wordsMatchLim + 1 < model.HeadItems.Count &&
                IsItemMatch<HeadWordItem>(model.HeadItems[wordsMatchLim + 1], words[wordsMatchLim], culture))
            {
                wordsMatchLim++;
            }

            if (wordsMatchLim + 1 < model.HeadItems.Count &&
                model.HeadItems[wordsMatchLim + 1] is HeadWordItem)
            {
                // We have a word we don't want, so truncate.
                action = ApplicationRobotAction.CreateHead(wordsMatchLim);
            }
            else if (wordsMatchLim == words.Length)
            {
                // We have all the words in the head we want.
                if (!complete)
                {
                    // We're done.
                    action = null;
                }
                else if (wordsMatchLim + 1 < model.HeadItems.Count &&
                    model.HeadItems[wordsMatchLim + 1] is GhostStopItem)
                {
                    // Click item in head to complete selection.
                    action = ApplicationRobotAction.CreateHeadAndComplete(wordsMatchLim + 1);
                }
                else
                {
                    // Click in tail to complete selection.

                    Debug.Assert(model.HeadItems.Count <= wordsMatchLim + 1 ||
                        model.HeadItems[wordsMatchLim + 1] is GhostWordItem);
                    Debug.Assert(model.TailItems.Count == 1);
                    Debug.Assert(model.TailItems[0] is TailStopItem);

                    action = ApplicationRobotAction.CreateTailAndComplete(0);
                }
            }
            else
            {
                // See how many ghost items match.
                var ghostMatchLim = wordsMatchLim;
                while (ghostMatchLim < words.Length &&
                    ghostMatchLim + 1 < model.HeadItems.Count &&
                    IsItemMatch<GhostWordItem>(model.HeadItems[ghostMatchLim + 1], words[ghostMatchLim], culture))
                {
                    ghostMatchLim++;
                }

                // Have we got enough ghost words to complete?
                if (complete &&
                    ghostMatchLim == words.Length &&
                    ghostMatchLim + 1 < model.HeadItems.Count &&
                    model.HeadItems[ghostMatchLim + 1] is GhostStopItem)
                {
                    // Complete by clicking ghost tail item.
                    action = ApplicationRobotAction.CreateHeadAndComplete(ghostMatchLim + 1);
                }
                else
                {
                    // Click something in the suggestions to advance.
                    action = GetSuggestionsAction(model, complete, words, wordsMatchLim, culture);

                    if (!action.IsComplete && wordsMatchLim < ghostMatchLim)
                    {
                        if (action.Target != ApplicationRobotActionTarget.Suggestion ||
                            action.SubIndex <= ghostMatchLim - wordsMatchLim)
                        {
                            // We would have been no better off than clicking in the ghost words, so do that.
                            action = ApplicationRobotAction.CreateHead(ghostMatchLim);
                        }
                    }
                }
            }

            return action;
        }

        /// <summary>
        /// Get the next action to achieve the given goal and press a Stop button to complete,
        /// </summary>
        /// <param name="model">The model to act against.</param>
        /// <param name="words">The words to be spoken.</param>
        /// <returns>The next action to take with IsComplete set true if this is the action to complete the goal.</returns>
        public static ApplicationRobotAction GetNextCompletionAction(ApplicationModel model, params string[] words)
        {
            var action = FindNextAction(model, true, words);
            return action;
        }

        /// <summary>
        /// Get the next action to achieve the given goal.
        /// </summary>
        /// <param name="model">The model to act against.</param>
        /// <param name="words">The words to be spoken.</param>
        /// <returns>The next action to take or null if no action is needed..</returns>
        public static ApplicationRobotAction GetNextEstablishingAction(ApplicationModel model, params string[] words)
        {
            var action = FindNextAction(model, false, words);
            return action;
        }
    }
}
