namespace KironTest.API.Hosting
{
    /// <summary>
    /// Runs a user-supplied async callback on a regular interval.
    /// </summary>
    public class TimerHostedService : IHostedService, IDisposable
    {
        private readonly Func<object, Task> _callback;
        private readonly TimeSpan _dueTime;
        private readonly TimeSpan _period;
        private Timer? _timer;

        /// <summary>
        /// </summary>
        /// <param name="callback">
        ///   async method to invoke on a schedule
        /// </param>
        /// <param name="dueTime">
        ///   how long to wait before the first call
        /// </param>
        /// <param name="period">
        ///   interval between subsequent calls
        /// </param>
        public TimerHostedService(Func<object, Task> callback, TimeSpan dueTime, TimeSpan period)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
            _dueTime = dueTime;
            _period = period;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(
                async state =>
                {
                    try
                    {
                        await _callback(state);
                    }
                    catch
                    {
                        // TODO: add logging here
                    }
                },
                state: null,
                dueTime: _dueTime,
                period: _period
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
