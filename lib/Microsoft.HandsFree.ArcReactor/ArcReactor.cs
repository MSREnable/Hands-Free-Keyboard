using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Reflecta;
using Reflecta.Interfaces;

namespace Microsoft.HandsFree.ArcReactor
{
    public class ArcReactor : INotifyPropertyChanged
    {
        private ArcReactorState _currentState;

        private const int Retries = 10;

        private readonly TraceSource _trace = new TraceSource("ArcReactor");
        private ReflectaClient _reflecta;
        // ReSharper disable once NotAccessedField.Local
        private Timer _timer;
        private dynamic _arcr1;

        private bool _upgradeAttempted;

        public uint[] Pixels = new uint[35];

        #region Initialization

        public void Initialize()
        {
            ReflectaClientManager.FindReflecta(arcr1.InterfaceName).ContinueWith(InitializeArcReactor).ContinueWith(t =>
            {
                if (_arcr1 != null)
                {
                    Connected = true;
                }
            });
        }

        async void InitializeArcReactor(Task<IEnumerable<ReflectaClient>> previousTask)
        {
            try
            {
                _reflecta = previousTask.Result.FirstOrDefault();
            }
            catch (AggregateException)
            { 
                _reflecta = null;
            }

            if (_reflecta == null)
            {
                _trace.TraceInformation("No ArcReactor connected -- arcr1 not found");
                return;
            }

            _trace.TraceInformation($"Connected to {_reflecta.Port.ComName} {_reflecta.Version().Result}");
            _arcr1 = _reflecta;

            _timer = new Timer(state => { _arcr1.keepAlive(); }, null, 0, 333);

            var success = false;
            for (var i = 0; i < Retries; i++)
            {
                try
                {
                    await _arcr1.setLedState(ArcReactorState.Idle);
                    success = true;
                }
                catch (Exception ex)
                {
                    _trace.TraceEvent(TraceEventType.Error, 0, ex.Message);
                }

                if (success)
                {
                    break;
                }
            }

            if (!success)
            {
                _trace.TraceInformation("Unable to set LED state");
            }
        }

        public void Dispose()
        {
            if (_reflecta != null)
            {
                _connected = false;

                _timer.Dispose();
                _timer = null;

                _reflecta.Dispose();
                _reflecta = null;
                _arcr1 = null;
            }
        }

        private bool _connected;
        public bool Connected {
            get {return _connected; }
            private set {
                if (_connected != value)
                {
                    _connected = value;
                    OnPropertyChanged();
                    OnConnectedChanged(EventArgs.Empty);
                }
            }
        }

        private void OnConnectedChanged(EventArgs e)
        {
            ConnectedChanged?.Invoke(this, e);
        }

        public event EventHandler ConnectedChanged;
        #endregion // Initialization

        public Task<string> Version => _arcr1?.Version();

        public bool NeedsUpgrade()
        {
            var needsUpgrade = false;
            var currentVersion = Version.Result;
            var gen2 = currentVersion.Contains("gen2");

            // Only support upgrade on Gen2 ArcReactor at the moment
            needsUpgrade = gen2 && currentVersion != Arduino_Gen2.Version.CurrentVersion;

            return needsUpgrade;
        }

        public void Upgrade()
        {
            if (_upgradeAttempted)
            {
                // We don't want to get into an upgrade loop
                // if the upgrade failed previously
                return;
            }

            _upgradeAttempted = true;

            var currentVersion = Version.Result;
            var gen2 = currentVersion.Contains("gen2");

            var arguments = gen2
                ? $"/C set TY_EXPERIMENTAL_BOARDS=true & tyc.exe upload Microsoft.HandsFree.ArcReactor.Arduino_Gen2\\{Arduino_Gen2.Version.CurrentVersion}.hex"
                : $"/C avrdude -Cavrdude.conf -v -patmega32u4 -cavr109 -P{_reflecta.Port.ComName} -b57600 -D -Uflash:w:Microsoft.HandsFree.ArcReactor.Arduino\\{Arduino.Version.CurrentVersion}.hex:i";

            Dispose(); // disconnect from the com port

            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = arguments
                };
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }

            Initialize();
        }

        #region Properties
        public ArcReactorState CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState != value && _currentState != ArcReactorState.Attendant)
                {
                    SetCurrentState(value);
                    OnPropertyChanged();
                }
            }
        }

        public bool AttendantCalled
        {
            get { return _currentState == ArcReactorState.Attendant; }
            set { SetCurrentState(value ? ArcReactorState.Attendant : ArcReactorState.Idle); }
        }

        private void SetCurrentState(ArcReactorState value)
        {
            _currentState = value;

            _arcr1?.setLedState(value);
        }
        #endregion // Properties

        public void ShowImage()
        {
            _arcr1?.showImage(Pixels);
        }

        public void ShowImageColor()
        {
            _arcr1?.showImageColor(Pixels);
        }

        public void SetPixel(byte index, uint pixelData)
        {
            _arcr1?.showPixel(index, pixelData);
        }

        public void SetPixelColor(byte index, byte red, byte green, byte blue)
        {
            _arcr1?.showPixelColor(index, red, green, blue);
        }

        public void ShowAnimation(byte index)
        {
            _arcr1?.showAnimation(index);
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion // INotifyPropertyChanged
    }
}
