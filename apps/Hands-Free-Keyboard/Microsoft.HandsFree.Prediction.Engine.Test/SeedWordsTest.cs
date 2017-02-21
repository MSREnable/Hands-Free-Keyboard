using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.HandsFree.Prediction.Engine.Test
{
    [TestClass]
    public class SeedWordsTest
    {
        [TestMethod]
        public void CheckSeedWords()
        {
            const int count = 7;

            using (var enumerator = WordScorePairEnumerable.Instance.GetEnumerator())
            {
                var index = 0;

                while (index < count && enumerator.MoveNext())
                {
                    var word = enumerator.Current.Word;

                    if (WordHelper.IsSuggestableWord(word))
                    {
                        Assert.AreEqual(WordSource.SeedWords[index], word);
                        //Debug.WriteLine("\"" + word + "\",");

                        index++;
                    }
                }

                var letterCounts = new int[26];
                var lettersFullyPopulated = 0;

                while (lettersFullyPopulated < 26 && enumerator.MoveNext())
                {
                    var word = enumerator.Current.Word;

                    var firstLetterIndex = char.ToLowerInvariant(word[0]) - 'a';

                    Assert.IsTrue(WordHelper.IsSuggestableWord(word));
                    if (letterCounts[firstLetterIndex] < count)
                    {
                        Assert.AreEqual(WordSource.SeedWords[index], word);
                        //Debug.WriteLine("\"" + word + "\",");

                        index++;

                        var newLetterCount = ++letterCounts[firstLetterIndex];

                        if (newLetterCount == count)
                        {
                            lettersFullyPopulated++;
                        }
                    }
                }

                Assert.AreEqual(WordSource.SeedWords.Length, index);
                Assert.AreEqual(count * (1 + 26), index);
            }
        }
    }
}
