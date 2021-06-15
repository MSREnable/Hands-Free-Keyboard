using Microsoft.Research.SpeechWriter.Core;
using Microsoft.Research.SpeechWriter.Core.Automation;
using System;
using System.Threading.Tasks;

namespace Microsoft.Research.SpeechWriter.UI
{
    public interface IApplicationHost
    {
        ApplicationModel Model { get; }

        TimeSpan MoveRectangeSeekTimeSpan { get; set; }

        TimeSpan MoveRectangeSettleTimeSpan { get; set; }

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
