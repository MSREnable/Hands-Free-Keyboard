using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Microsoft.Research.SpeechWriter.UI
{
    abstract class LayoutHelper<TControl, TSize, TRect, T>
        where TControl : class
        where TSize : struct
        where TRect : struct
    {
        private readonly ApplicationPanelHelper<TControl, TSize, TRect> _helper;
        private readonly ReadOnlyObservableCollection<T> _list;
        private readonly List<TControl> _controls = new List<TControl>();

        internal LayoutHelper(ApplicationPanelHelper<TControl, TSize, TRect> helper, ReadOnlyObservableCollection<T> list)
        {
            _helper = helper;
            _list = list;

            ((INotifyCollectionChanged)list).CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddContent(_list, e.NewStartingIndex, e.NewItems.Count);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    RemoveContent(_list, e.OldStartingIndex, e.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    ReplaceContent(_list, e.OldStartingIndex, e.OldItems.Count);
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                default:
                    ResetContent(_list);
                    break;
            }
        }

        protected virtual void AddContent(IList<T> list, int startIndex, int count)
        {
            ResetContent(list);
        }

        protected virtual void RemoveContent(IList<T> list, int startIndex, int count)
        {
            ResetContent(list);
        }

        protected virtual void ReplaceContent(IList<T> list, int startIndex, int count)
        {
            ResetContent(list);
        }

        protected virtual void ResetContent(IList<T> list)
        {
            while (_controls.Count != 0)
            {
                _helper._panel.ResetControls();
            }

            foreach (var tile in list)
            {
                //CreateItem(tile);
            }
        }
    }
}
