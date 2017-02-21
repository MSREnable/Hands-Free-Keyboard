namespace Microsoft.HandsFree.Keyboard.Controls.Layout
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Base class for layout context.
    /// </summary>
    interface ILayoutContext
    {
        /// <summary>
        /// Leftmost co-ordinate value.
        /// </summary>
        double Left { get; }

        /// <summary>
        /// Rightmost co-ordinate value.
        /// </summary>
        double Top { get; }

        /// <summary>
        /// Side length of standard key.
        /// </summary>
        double KeySize { get; }

        /// <summary>
        /// Set the current binding to that for the default.
        /// </summary>
        /// <param name="otherSelectable">All the other states specified by the optional section.</param>
        void SetDefaultBinding(ISet<string> otherSelectable);

        /// <summary>
        /// Set the binding to a named state.
        /// </summary>
        /// <param name="state">Name of the named state.</param>
        void SetNamedBinding(string state);

        /// <summary>
        /// Reset binding to general case.
        /// </summary>
        void ResetBinding();

        /// <summary>
        /// Create an (pseudo) gap key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        void CreateGapKey(GapKeyLayout layout, double left, double top, double width, double height);

        /// <summary>
        /// Create an character key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        void CreateCharacterKey(CharacterKeyLayout layout, double left, double top, double width, double height);

        /// <summary>
        /// Create an action key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        void CreateActionKey(ActionKeyLayout layout, double left, double top, double width, double height);

        /// <summary>
        /// Create a toggle key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        void CreateToggleKey(ToggleKeyLayout layout, double left, double top, double width, double height);

        /// <summary>
        /// Create a state key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        void CreateStateKey(StateKeyLayout layout, double left, double top, double width, double height);
    }

    /// <summary>
    /// Layout engine.
    /// </summary>
    public abstract class LayoutContext<TControl> : ILayoutContext
    {
        readonly KeyboardLayout layout;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="layout">The layout to layout.</param>
        /// <param name="totalWidth">Total width of layout space.</param>
        /// <param name="totalHeight">Total width of layout space.</param>
        /// <param name="keySize">The size of a standard key.</param>
        protected LayoutContext(KeyboardLayout layout, double totalWidth, double totalHeight, double keySize)
        {
            this.layout = layout;
            Left = (totalWidth - layout.KeyWidth * keySize) / 2;
            Top = (totalHeight - layout.KeyHeight * keySize) / 2;

            KeySize = keySize;

            Debug.Assert(allStates.Count == 0, "All available states has not been initialised");
            layout.GatherKeyboardStates(allStates);

            foreach (var state in allStates)
            {
                otherShowHides[state] = new StateShowHideContainer<TControl>();
            }
        }

        readonly StateShowHideContainer<TControl> defaultShowHide = new StateShowHideContainer<TControl>();

        readonly IDictionary<string, StateShowHideContainer<TControl>> otherShowHides = new Dictionary<string, StateShowHideContainer<TControl>>();

        readonly ISet<string> allStates = new HashSet<string>();
        readonly List<StateShowHideContainer<TControl>> showBindings = new List<StateShowHideContainer<TControl>>();
        readonly List<StateShowHideContainer<TControl>> hideBindings = new List<StateShowHideContainer<TControl>>();

        bool isBindingDefault = true;

        /// <summary>
        /// Leftmost co-ordinate value.
        /// </summary>
        public double Left { get; }

        /// <summary>
        /// Rightmost co-ordinate value.
        /// </summary>
        public double Top { get; }

        /// <summary>
        /// Side length of standard key.
        /// </summary>
        public double KeySize { get; }

        /// <summary>
        /// Create an (pseudo) gap key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        public void CreateGapKey(GapKeyLayout layout, double left, double top, double width, double height)
        {
            CreateGapControl(layout, left, top, width, height);
        }

        /// <summary>
        /// Create an instance of a control type.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        protected abstract void CreateGapControl(GapKeyLayout layout, double left, double top, double width, double height);

        /// <summary>
        /// Create a character key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        public void CreateCharacterKey(CharacterKeyLayout layout, double left, double top, double width, double height)
        {
            var control = CreateCharacterControl(layout, left, top, width, height, isBindingDefault);

            Bind(control);
        }

        /// <summary>
        /// Create an instance of the control type.
        /// </summary>
        /// <param name="layout">The layout information.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="isVisible">Is the control initially visible?</param>
        /// <returns>The control.</returns>
        protected abstract TControl CreateCharacterControl(CharacterKeyLayout layout, double left, double top, double width, double height, bool isVisible);

        /// <summary>
        /// Create an action key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        public void CreateActionKey(ActionKeyLayout layout, double left, double top, double width, double height)
        {
            var control = CreateActionControl(layout, left, top, width, height, isBindingDefault);

            Bind(control);
        }

        /// <summary>
        /// Create an instance of the control type.
        /// </summary>
        /// <param name="layout">The layout information.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="isVisible">Is the control initially visible?</param>
        /// <returns>The control.</returns>
        protected abstract TControl CreateActionControl(ActionKeyLayout layout, double left, double top, double width, double height, bool isVisible);

        /// <summary>
        /// Create a toggle key.
        /// </summary>
        /// <param name="layout">The key layout.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        public void CreateToggleKey(ToggleKeyLayout layout, double left, double top, double width, double height)
        {
            var control = CreateToggleControl(layout, left, top, width, height, isBindingDefault);

            Bind(control);
        }

        /// <summary>
        /// Create an instance of the control type.
        /// </summary>
        /// <param name="layout">The layout information.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="isVisible">Is the control initially visible?</param>
        /// <returns>The control.</returns>
        protected abstract TControl CreateToggleControl(ToggleKeyLayout layout, double left, double top, double width, double height, bool isVisible);

        /// <summary>
        /// Create a key.
        /// </summary>
        /// <param name="layout">Layout for key.</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        public void CreateStateKey(StateKeyLayout layout, double left, double top, double width, double height)
        {
            var list = layout.StateName == null ? defaultShowHide : otherShowHides[layout.StateName];

            var control = CreateStateControl(layout, list, left, top, width, height, isBindingDefault);

            Bind(control);
        }

        /// <summary>
        /// Create an instance of the control type.
        /// </summary>
        /// <param name="layout">The control layout.</param>
        /// <param name="list">The show/hide list</param>
        /// <param name="top">Top position within layout space.</param>
        /// <param name="left">Left position within layout space.</param>
        /// <param name="height">Bottom position within layout space.</param>
        /// <param name="width">Right position within layout space.</param>
        /// <param name="isVisible">Is the control initially visible?</param>
        /// <returns>The control.</returns>
        protected abstract TControl CreateStateControl(StateKeyLayout layout, StateShowHideContainer<TControl> list, double left, double top, double width, double height, bool isVisible);

        /// <summary>
        /// Bind to a created button.
        /// </summary>
        /// <param name="button">The created button.</param>
        void Bind(TControl button)
        {
            foreach (var binding in showBindings)
            {
                binding.ShowList.Add(button);
            }

            foreach (var binding in hideBindings)
            {
                binding.HideList.Add(button);
            }
        }

        /// <summary>
        /// Set the current binding to that for the default.
        /// </summary>
        /// <param name="otherSelectable">All the other states specified by the optional section.</param>
        public void SetDefaultBinding(ISet<string> otherSelectable)
        {
            ResetBinding();

            showBindings.Add(defaultShowHide);

            foreach (var pair in otherShowHides)
            {
                if (otherSelectable.Contains(pair.Key))
                {
                    hideBindings.Add(pair.Value);
                }
                else
                {
                    showBindings.Add(pair.Value);
                }
            }
        }

        /// <summary>
        /// Set the binding to a named state.
        /// </summary>
        /// <param name="state">Name of the named state.</param>
        public void SetNamedBinding(string state)
        {
            ResetBinding();

            isBindingDefault = false;

            hideBindings.Add(defaultShowHide);

            foreach (var pair in otherShowHides)
            {
                if (state == pair.Key)
                {
                    showBindings.Add(pair.Value);
                }
                else
                {
                    hideBindings.Add(pair.Value);
                }
            }
        }

        /// <summary>
        /// Reset binding to general case.
        /// </summary>
        public void ResetBinding()
        {
            showBindings.Clear();
            hideBindings.Clear();

            isBindingDefault = true;
        }

        /// <summary>
        /// Run the layout engine.
        /// </summary>
        public void Run()
        {
            layout.Layout(this);
        }
    }
}
