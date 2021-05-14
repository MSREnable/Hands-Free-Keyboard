﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Research.SpeechWriter.Core.Data.Test
{
    public class TileDataTest
    {
        [Test]
        public void IsSimpleWordTest()
        {
            {
                var tile = new TileData("Hello");
                Assert.IsTrue(tile.ToString() == "<T>Hello</T>");
                Assert.IsTrue(tile.IsSimpleWord);
            }

            var simples = new[] { "Hello", "This-is" };
            foreach (var simple in simples)
            {
                var tile = new TileData(simple);
                Assert.IsTrue(tile.IsSimpleWord);
            }

            foreach (var simple in simples)
            {
                var tile = new TileData(simple, isGlueBefore: true);
                Assert.IsFalse(tile.IsSimpleWord);
            }

            foreach (var simple in simples)
            {
                var tile = new TileData(simple, isGlueAfter: true);
                Assert.IsFalse(tile.IsSimpleWord);
            }

            var complexes = new[] { "Hello World", "&^%^" };
            foreach (var complex in complexes)
            {
                var tile = new TileData(complex);
                Assert.IsFalse(tile.IsSimpleWord);
            }
        }


        private static IEnumerable<TileData> ElementEnumerable
        {
            get
            {
                for (var i = 0; i < 2; i++)
                {
                    for (var j = 0; j < 2; j++)
                    {
                        for (var k = 0; k < 2; k++)
                        {
                            var element = new TileData(i.ToString(), k != 0, j != 0);
                            yield return element;
                        }
                    }
                }
            }
        }

        [Test]
        public void TileSetTest()
        {
            var count = ElementEnumerable.Count();
            Assert.AreEqual(8, count);
            var elements = new HashSet<TileData>(ElementEnumerable);
            Assert.AreEqual(count, elements.Count());

            foreach (var element in ElementEnumerable)
            {
                Assert.IsTrue(elements.Contains(element));
            }
        }

        [Test]
        public void TileEqualityTest()
        {
            var unmatchable = new TileData("TEST");
            var matchable = new TileData("1");
            var plainWrong = "Wibble";

            var matchCount = 0;
            foreach (var element in ElementEnumerable)
            {
                Assert.IsFalse(unmatchable.Equals(element));
                if (matchable.Equals(element))
                {
                    matchCount++;
                }
                Assert.IsFalse(plainWrong.Equals(element));

                Assert.AreEqual(element.Equals(matchable), matchable.Equals(element));
                Assert.AreEqual(element.Equals(unmatchable), unmatchable.Equals(element));
                Assert.AreEqual(element.Equals(plainWrong), plainWrong.Equals(element));
            }
            Assert.AreEqual(1, matchCount);
        }

        [Test]
        public void NoNullInContentTest()
        {
            var contents = new[] { "Good", "Bad\0", "\0Badder", "Bad\0est" };
            for (var i = 0; i < contents.Length; i++)
            {
                var good = true;
                try
                {
                    _ = new TileData(contents[i]);
                }
                catch (ArgumentException)
                {
                    good = false;
                }
                Assert.AreEqual(i == 0, good);
            }
        }

        [Test]
        public void CheckTokenizationTest()
        {
            CheckTokenization(new TileData("Simple"), "Simple");
            CheckTokenization(new TileData("Before", isGlueAfter: true), "Before\0B");
            CheckTokenization(new TileData("After", isGlueBefore: true), "After\0A");
            CheckTokenization(new TileData("Before", isGlueBefore: true, isGlueAfter: true), "Before\0J");

            void CheckTokenization(TileData tile, string expected)
            {
                var token = tile.ToTokenString();
                Assert.AreEqual(expected, token);

                var reborn = TileData.FromTokenString(token);
                Assert.AreEqual(tile, reborn);
            }
        }
    }
}
