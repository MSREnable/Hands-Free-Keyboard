using NUnit.Framework;
using System;

namespace Microsoft.Research.SpeechWriter.Core.UI.Test
{
    public class SimpleStartUi

    {
        [Test]
        public void SimpleCreation()
        {
            var surface = new MockButtonSurfaceUI();
            var application = new ApplicationLayout<MockButton>(surface, 8, 2);

            surface.RaiseResize(11, 8);

            surface.InvokeButtonByGrid(6, 0.5);
        }

        [Test]
        public void NoApplicationCreation()
        {
            var surface = new MockButtonSurfaceUI();
            surface.RaiseResize(1, 2);

            var resizeCount = 0;

            IButtonSurfaceUI<MockButton> buttonSurface = surface;
            buttonSurface.Resized += CountResizes;

            surface.RaiseResize(3, 4);

            Assert.AreEqual(1, resizeCount);

            buttonSurface.Resized -= CountResizes;

            surface.RaiseResize(5, 6);

            Assert.AreEqual(1, resizeCount);

            void CountResizes(object sender, EventArgs e)
            {
                resizeCount++;
            }
        }
    }
}