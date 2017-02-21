using Microsoft.HandsFree.ArcReactor;
using Microsoft.HandsFree.Keyboard.Model;
using System;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    /// <summary>
    /// Implementation of IActivityDisplayProvider.
    /// </summary>
    public class ActivityDisplayProvider : IActivityDisplayProvider
    {
        readonly ArcReactor.ArcReactor _arcReactor = new ArcReactor.ArcReactor();

        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly IActivityDisplayProvider Instance = new ActivityDisplayProvider();

        /// <summary>
        /// Private constructor.
        /// </summary>
        ActivityDisplayProvider()
        {
            _arcReactor.ConnectedChanged += delegate
            {
                if (_arcReactor.Connected)
                {
                    _isOn = true;
                    _displayStatus = _arcReactor.Version.Result;

                    if (_arcReactor.NeedsUpgrade())
                    {
                        _arcReactor.Upgrade();
                    }
                }
                else
                {
                    _isOn = false;
                    _displayStatus = "Secondary Display Not Connected";
                }

                _statusChanged?.Invoke(this, EventArgs.Empty);
            };
            _arcReactor.Initialize();
        }

        bool IActivityDisplayProvider.IsOn { get { return _isOn; } set { _isOn = value; SetDisplayStatus(); } }
        bool _isOn;

        bool IActivityDisplayProvider.IsTyping { get { return _isTyping; } set { _isTyping = value; SetDisplayStatus(); } }
        bool _isTyping;

        bool IActivityDisplayProvider.IsSpeaking { get { return _isSpeaking; } set { _isSpeaking = value; SetDisplayStatus(); } }
        bool _isSpeaking;

        string IActivityDisplayProvider.Status => _displayStatus;
        private string _displayStatus = "No Secondary Display Found";

        event EventHandler IActivityDisplayProvider.StatusChanged { add { _statusChanged += value; } remove { _statusChanged -= value; } }
        EventHandler _statusChanged;

        void SetDisplayStatus()
        {
            ArcReactorState state;

            if (_isOn)
            {
                if (_isSpeaking)
                {
                    state = ArcReactorState.Talking;
                }
                else if (_isTyping)
                {
                    state = ArcReactorState.Typing;
                }
                else
                {
                    state = ArcReactorState.Idle;
                }
            }
            else
            {
                state = ArcReactorState.Off;
            }

            _arcReactor.CurrentState = state;
        }
    }
}
