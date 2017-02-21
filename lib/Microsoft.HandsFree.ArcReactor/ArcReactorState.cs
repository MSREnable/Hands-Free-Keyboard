namespace Microsoft.HandsFree.ArcReactor
{
    public enum ArcReactorState : byte
    {
        Off = 0,
        Typing,
        Talking,
        Emergency,
        Attendant,
        HandRaise,
        Smile,
        Idle,
        DirectionLeft,
        DirectionRight,
        DirectionUp,
        DirectionDown,
        ShowImage,
        Animation,
    }
}
