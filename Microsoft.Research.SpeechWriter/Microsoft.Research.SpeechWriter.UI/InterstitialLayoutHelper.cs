using Microsoft.Research.SpeechWriter.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Research.SpeechWriter.UI
{
    class InterstitialLayoutHelper<TControl, TSize, TRect> : LayoutHelper<TControl, TSize, TRect, ITile>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        private readonly ApplicationPanelHelper<TControl, TSize, TRect> _panel;

        internal InterstitialLayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> panel,
            ReadOnlyObservableCollection<ITile> list)
            : base(panel, list)
        {
        }
    }
}
