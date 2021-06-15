using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Microsoft.Research.SpeechWriter.DemoAppWpf
{
    public interface IApplicationHost
    {
        ApplicationModel Model { get; }

        KeyTime MoveRectangeSeekTime { get; set; }

        KeyTime MoveRectangeSettleTime { get; set; }

        event KeyEventHandler PreviewKeyDown;

        void ShowTargetOutline();

        void HideTargetOutline();

        Task<string> GetClipboardStringAsync();

        void SetupStoryboardForAction(ApplicationRobotAction action);

        Task PlayMoveRectangleAsync();

        Task PlayTutorMoveStoryboardAsync();

        void Restart(bool loadHistory);

        void ShowLogging();
    }
}
