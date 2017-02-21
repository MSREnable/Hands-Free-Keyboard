namespace Microsoft.HandsFree.SlackClient
{
    public static class SlackClientFactory
    {
        public static IUtteranceTarget Create()
        {
            var target = new SlackUtteranceTarget();
            return target;
        }
    }
}
