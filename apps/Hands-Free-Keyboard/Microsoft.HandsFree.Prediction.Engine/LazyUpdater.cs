namespace Microsoft.HandsFree.Prediction.Engine
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    public class LazyUpdater
    {
        readonly Action<Action> queueWorkItem;

        /// <summary>
        /// The next action to be performed. This will be null when the worker thread 
        /// is idle (or is contemplating going idle), OnGoingIdleAsync when the thread
        /// is running its last scheduled action or something else when that is the
        /// next thing to be done.
        /// </summary>
        Func<Task> pendingAction;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queueWorkItem">Method to queue a work item.</param>
        public LazyUpdater(Action<Action> queueWorkItem)
        {
            this.queueWorkItem = queueWorkItem;
        }

        /// <summary>
        /// Called when worker thread starts before new sequence of updates.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnThreadStartAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Runs before one or more updater actions take place.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnBusyStart()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Runs after one or more updater actions have taken place when another
        /// updater is not immediately going to start.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnBusyStop()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called when worked thread stops after a sequence of updates.
        /// </summary>
        /// <returns></returns>
        protected virtual Task OnThreadStopAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Background thread handler.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        async Task UserWorkItemAsync(object parameter)
        {
            await OnThreadStartAsync();

            Func<Task> action = OnBusyStop;

            do
            {
                // Wind up processing environment.
                await OnBusyStart();

                // While we have not arrived at the OnGoingIdleAsync action.
                do
                {
                    // Pick up the first action, replace it with the wind down action.
                    var currentAction = Interlocked.Exchange(ref pendingAction, OnBusyStop);

                    // We have one or two things to do, do the most recent one that is not to do nothing!
                    Debug.Assert(action != OnBusyStop || currentAction != OnBusyStop);
                    if (currentAction == null || currentAction == OnBusyStop)
                    {
                        currentAction = action;
                    }

                    // Perform the action.
                    await currentAction();

                    // Pick up the next action.
                    action = Interlocked.Exchange(ref pendingAction, OnBusyStop);
                }
                while (action != OnBusyStop);

                // Perform the wind down.
                await OnBusyStop();

                // Check if we need to ressurect the background thread.
                action = Interlocked.Exchange(ref pendingAction, null);
            }
            while (action != OnBusyStop);

            await OnThreadStopAsync();
        }

        public void QueueUpdate(Func<Task> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            var queuedAction = Interlocked.Exchange(ref pendingAction, action);

            if (queuedAction == null)
            {
                queueWorkItem(() => { UserWorkItemAsync(null).Wait(); });
            }
        }
    }
}
