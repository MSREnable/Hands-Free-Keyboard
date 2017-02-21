using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Prediction.Historic.Test
{
    [TestClass]
    public class Cases
    {
        static Plumber<double> CreatePlumber(double[] depths)
        {
            var limit = depths.Length;
            Assert.IsTrue(0 < limit, "Must be at least one value");

            // Start assing first position is maximum depth.
            var depth = depths[0];
            var position = 1;

            // Work down to edge of supposed bottom.
            while (position < limit && depth < depths[position])
            {
                depth = depths[position];

                position++;
            }

            var maxPosition = position - 1;

            // Work way back up.
            while (position < limit)
            {
                Assert.IsTrue(depths[position] < depth);
                depth = depths[position];

                position++;
            }

            Assert.AreEqual(limit, position);

            var plumber = Plumber<double>.Create(1, (r, i) => (int)depths[i], (r, i) => maxPosition < i ? -1 : maxPosition == i ? 0 : 0, limit);

            Assert.AreEqual((int)depths[maxPosition], plumber.MaxDepth);

            return plumber;
        }

        static void CheckSlices(double[] depths, Plumber<double> plumber)
        {
            var enumerable = plumber.GetDepthSlices();
            using (var enumerator = enumerable.GetEnumerator())
            {
                var prevStart = int.MaxValue;
                var prevLimit = int.MinValue;

                for (var depth = plumber.MaxDepth; 0 <= depth; depth--)
                {
                    var depthStart = 0;
                    while (depths[depthStart] < depth)
                    {
                        depthStart++;
                    }

                    var depthLimit = depths.Length;
                    while (depths[depthLimit - 1] < depth)
                    {
                        depthLimit--;
                    }

                    var slice = plumber.GetDepthSlice(depth);
                    Assert.AreEqual(depthStart, slice.Start);
                    Assert.AreEqual(depthLimit, slice.Limit);

                    if (depthStart != prevStart || depthLimit != prevLimit)
                    {
                        Assert.IsTrue(enumerator.MoveNext(), "Another slice is available");
                        Assert.AreEqual(depthStart, enumerator.Current.Start, "Start of enumerated slices matches");
                        Assert.AreEqual(depthLimit, enumerator.Current.Limit, "Limit of enumerated slices matches");

                        prevStart = depthStart;
                        prevLimit = depthLimit;
                    }
                }

                Assert.IsFalse(enumerator.MoveNext(), "Reached end of slices");
            }
        }

        static void Check(params double[] depths)
        {
            var plumber = CreatePlumber(depths);

            CheckSlices(depths, plumber);
        }

        [TestMethod]
        public void Simple()
        {
            Check(0, 1, 0);
        }

        [TestMethod]
        public void SunkenSimple()
        {
            Check(1, 2, 1);
        }

        [TestMethod]
        public void SplitPositions0()
        {
            Check(0);
        }
        [TestMethod]
        public void SplitPositions01()
        {
            Check(0, 1);
        }
        [TestMethod]
        public void SplitPositions10()
        {
            Check(1, 0);
        }
        [TestMethod]
        public void SplitPositions210()
        {
            Check(2, 1, 0);
        }
        [TestMethod]
        public void SplitPositions010()
        {
            Check(0, 1, 0);
        }
        [TestMethod]
        public void SplitPositions012()
        {
            Check(0, 1, 2);
        }
        [TestMethod]
        public void SplitPositions3210()
        {
            Check(3, 2, 1, 0);
        }
        [TestMethod]
        public void SplitPositions0210()
        {
            Check(0, 2, 1, 0);
        }
        [TestMethod]
        public void SplitPositions0120()
        {
            Check(0, 1, 2, 0);
        }
        [TestMethod]
        public void SplitPositions0123()
        {
            Check(0, 1, 2, 3);
        }
    }
}
