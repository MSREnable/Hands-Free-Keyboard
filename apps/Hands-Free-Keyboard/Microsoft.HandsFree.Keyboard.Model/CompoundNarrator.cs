namespace Microsoft.HandsFree.Keyboard.Model
{
    class CompoundNarrator : INarrator
    {
        readonly INarrator[] _providers;

        internal CompoundNarrator(params INarrator[] providers)
        {
            _providers = providers;
        }

        void INarrator.OnNarrationEvent(NarrationEventArgs e)
        {
            foreach(var provider in _providers)
            {
                provider.OnNarrationEvent(e);
            }
        }
    }
}
