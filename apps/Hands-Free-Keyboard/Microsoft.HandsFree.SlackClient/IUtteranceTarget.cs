namespace Microsoft.HandsFree.SlackClient
{
    using System.Threading.Tasks;

    public interface IUtteranceTarget
    {
        void Send(string utterance);

        Task SendAsync(string utterance);
    }
}
