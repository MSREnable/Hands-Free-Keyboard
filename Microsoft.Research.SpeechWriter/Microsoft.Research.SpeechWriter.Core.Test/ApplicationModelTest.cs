using NUnit.Framework;

namespace Microsoft.Research.SpeechWriter.Core.Test
{
    [Parallelizable(ParallelScope.All)]
    public class ApplicationModelTest
    {
        [Test]
        public void ShowTestCardTest()
        {
            var model = new ApplicationModel();
            model.ShowTestCard();
            model.ShowTestCard();
        }
    }
}
