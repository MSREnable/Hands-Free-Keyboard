using System.Diagnostics;
using System.Windows.Input;

namespace Microsoft.Research.SpeechWriter.Core.Automation
{
    /// <summary>
    /// Description of the next step towards the target.
    /// </summary>
    public class ApplicationRobotAction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="index"></param>
        /// <param name="subIndex"></param>
        /// <param name="isComplete"></param>
        public ApplicationRobotAction(ApplicationRobotActionTarget target,
            int index,
            int subIndex,
            bool isComplete)
        {
            Debug.Assert(0 <= index);
            Debug.Assert(target == ApplicationRobotActionTarget.Suggestion || subIndex == 0);

            Target = target;
            Index = index;
            SubIndex = subIndex;
            IsComplete = isComplete;
        }

        private ApplicationRobotAction(ApplicationRobotActionTarget target,
            int index,
            int subIndex)
            : this(target, index, subIndex, false)
        {
        }

        private ApplicationRobotAction(ApplicationRobotActionTarget target,
            int index,
            bool isComplete)
            : this(target, index, 0, isComplete)
        {
        }

        private ApplicationRobotAction(ApplicationRobotActionTarget target,
            int index)
            : this(target, index, 0, false)
        {
        }

        internal static ApplicationRobotAction CreateHead(int index) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index);

        internal static ApplicationRobotAction CreateHeadAndComplete(int index) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Head, index, true);

        internal static ApplicationRobotAction CreateTail(int index) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, index);

        internal static ApplicationRobotAction CreateTailAndComplete(int index) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Tail, index, true);

        internal static ApplicationRobotAction CreateSuggestion(int index, int subIndex) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, subIndex);

        internal static ApplicationRobotAction CreateSuggestionAndComplete(int index, int subIndex) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Suggestion, index, subIndex, true);

        internal static ApplicationRobotAction CreateInterstitial(int index) =>
            new ApplicationRobotAction(ApplicationRobotActionTarget.Interstitial, index);

        /// <summary>
        /// The list in the application model that is targetted.
        /// </summary>
        public ApplicationRobotActionTarget Target { get; }

        /// <summary>
        /// The item within the targetted list that needs to be executed.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// For the suggestions list of lists only, the item within the selected list that need to be executed.
        /// </summary>
        public int SubIndex { get; }

        /// <summary>
        /// This action results in completion of the sought sequence.
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        /// Get the model item.
        /// </summary>
        /// <param name="model">The model to select from.</param>
        /// <returns>The item from the model.</returns>
        public ICommand GetItem(ApplicationModel model)
        {
            ICommand item;

            switch (Target)
            {
                case ApplicationRobotActionTarget.Head:
                    item = model.HeadItems[Index];
                    break;

                case ApplicationRobotActionTarget.Tail:
                    item = model.TailItems[Index];
                    break;

                case ApplicationRobotActionTarget.Interstitial:
                    item = model.SuggestionInterstitials[Index];
                    break;

                case ApplicationRobotActionTarget.Suggestion:
                default:
                    Debug.Assert(Target == ApplicationRobotActionTarget.Suggestion);

                    var list = model.SuggestionLists[Index];

                    using (var enumerator = list.GetEnumerator())
                    {
                        var i = 0;
                        do
                        {
                            var moved = enumerator.MoveNext();
                            Debug.Assert(moved);

                            i++;
                        }
                        while (i <= SubIndex);

                        item = enumerator.Current;
                    }
                    break;
            }

            return item;
        }

        /// <summary>
        /// Execute the model item.
        /// </summary>
        /// <param name="model">THe model to execute from.</param>
        public void ExecuteItem(ApplicationModel model)
        {
            var item = GetItem(model);
            Debug.Assert(item.CanExecute(null));
            item.Execute(null);
        }
    }
}
