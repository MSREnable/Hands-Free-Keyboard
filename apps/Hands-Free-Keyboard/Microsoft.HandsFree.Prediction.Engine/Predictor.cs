using Microsoft.HandsFree.Prediction.Api;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.HandsFree.Prediction.Engine
{
    class Predictor : IPredictor
    {
        readonly IPredictionEnvironment _environment;

        readonly LazyUpdater _updater;

        readonly Action<Predictor> _predictorUpdater;

        IWordSuggester _wordSuggester;

        IPhraseSuggester _phraseSuggester;

        readonly SemaphoreSlim _newHistorySemaphore = new SemaphoreSlim(1);

        readonly List<string> _newHistoryList = new List<string>();

        internal Predictor(IPredictionEnvironment environment, IWordSuggester wordSuggester, IPhraseSuggester phraseSuggester, Action<Predictor> predictorUpdater)
        {
            _updater = new LazyUpdater(environment.QueueWorkItem);

            _environment = environment;
            _wordSuggester = wordSuggester;
            _phraseSuggester = phraseSuggester;
            _predictorUpdater = predictorUpdater;
        }

        internal IPredictionEnvironment Environment { get { return _environment; } }

        public IPrediction CreatePrediction(string text, int selectionStart, int selectionLength, bool isAutoSpace, object hints)
        {
            var prediction = new Prediction(_environment, _wordSuggester, _phraseSuggester, text, selectionStart, selectionLength, isAutoSpace);
            return prediction;
        }

        public void RecordHistory(string text, bool isInPrivate)
        {
            _environment.RecordHistory(text, isInPrivate);

            if (!isInPrivate)
            {
                try
                {
                    _newHistorySemaphore.Wait();
                    _newHistoryList.Add(text);
                }
                finally
                {
                    _newHistorySemaphore.Release();
                }

                QueueUpdate();
            }
        }

        internal IEnumerable<string> ConsumeNewHistory()
        {
            var newHistory = new List<string>();

            try
            {
                _newHistorySemaphore.Wait();

                newHistory.AddRange(_newHistoryList);
                _newHistoryList.Clear();
            }
            finally
            {
                _newHistorySemaphore.Release();
            }

            return newHistory;
        }

        internal void UpdateConfiguration(IWordSuggester wordSuggester, IPhraseSuggester phraseSuggester)
        {
            _wordSuggester = wordSuggester;
            _phraseSuggester = phraseSuggester;

            var predictionChangedHandler = PredictionChanged;
            if (predictionChangedHandler != null)
            {
                predictionChangedHandler(this, EventArgs.Empty);
            }
        }

        public event EventHandler PredictionChanged;

        internal void QueueUpdate()
        {
            _updater.QueueUpdate(() => { _predictorUpdater(this); return Task.FromResult(0); });
        }
    }
}

