﻿using Windows.UI.Xaml.Controls;

namespace Microsoft.Research.SpeechWriter.Core.UI.Uwp
{
    public class ButtonUI : Button, IButtonUI
    {
        double IButtonUI.RenderedWidth => DesiredSize.Width;
    }
}
