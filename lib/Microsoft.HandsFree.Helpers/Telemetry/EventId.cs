namespace Microsoft.HandsFree.Helpers.Telemetry
{
    /// <summary>
    /// Well known event ids
    /// </summary>
    public enum EventId
    {
        AppStart = 1,
        AppStop,
        MinutesOfActiveUse = 10,
        WordsSpoken = 20,
        PhrasesSpoken = 30,
        Actions = 100,
        Performance = 200,
        WordsPerMinute,
        Surveys = 300,
        Tlx = 301
    }
}
