namespace Microsoft.HandsFree.Prediction.Engine
{
    using Microsoft.HandsFree.Prediction.Api;
    using System;
    using System.Threading.Tasks;

    class CharacterSuggestion : IPredictionSuggestion
    {
        internal CharacterSuggestion(char ch, double probability)
        {
            Confidence = probability;
            Text = ch.ToString();
        }

        public double Confidence
        {
            get;
            internal set;
        }

        public string Text
        {
            get;
            internal set;
        }

        public int ReplacementStart
        {
            get { throw new NotImplementedException(); }
        }

        public int ReplacementLength
        {
            get { throw new NotImplementedException(); }
        }

        public bool CompleteWord
        {
            get { return false; }
        }

        public void Accepted(int index)
        {
        }
    }
}

