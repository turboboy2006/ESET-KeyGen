using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RankTracker.Core.Data;
using RankTracker.Core.Services;
using RankTracker.Core.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RankTracker.WebAPI.Services
{
    public class ScheduledRankCheckService : IHostedService, IDisposable
    {
        private readonly ILogger<ScheduledRankCheckService> _logger;
        private Timer _timer;
        private readonly IServiceProvider _serviceProvider;

        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);
        private readonly TimeSpan _runAtTimeOfDay = new TimeSpan(2, 0, 0); // 2:00 AM, configurable
        private DateTime _lastRunDate = DateTime.MinValue;

        public ScheduledRankCheckService(ILogger<ScheduledRankCheckService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scheduled Rank Check Service is starting.");

            // Calculate an appropriate initial delay.
            // Aim to run at the next _runAtTimeOfDay, then check based on _checkInterval.
            var now = DateTime.Now;
            var nextScheduledRun = DateTime.Today.Add(_runAtTimeOfDay);
            if (now > nextScheduledRun)
            {
                nextScheduledRun = nextScheduledRun.AddDays(1); // If past today's time, aim for tomorrow
            }

            TimeSpan initialDelay = nextScheduledRun - now;
            if (initialDelay < TimeSpan.Zero) initialDelay = TimeSpan.Zero; // Ensure non-negative

            _timer = new Timer(DoWorkWrapper, null, initialDelay, _checkInterval);
            _logger.LogInformation($"Service started. Initial check scheduled at {DateTime.Now.Add(initialDelay)}. Subsequent checks every {_checkInterval}.");

            return Task.CompletedTask;
        }

        // Wrapper to allow async void DoWork to be called by Timer
        private void DoWorkWrapper(object state)
        {
            try
            {
                DoWorkAsync(state).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in DoWorkWrapper for ScheduledRankCheckService.");
            }
        }

        private async Task DoWorkAsync(object state)
        {
            var now = DateTime.Now;
            _logger.LogInformation($"ScheduledRankCheckService: Timer ticked at {now}. Checking run conditions.");

            // Check if it's the correct time of day and if it hasn't run for the current date yet.
            // Using >= allows for some flexibility if the timer callback is slightly delayed.
            if (now.TimeOfDay >= _runAtTimeOfDay && now.Date > _lastRunDate.Date)
            {
                _logger.LogInformation($"Attempting daily rank checks at {now}. Last run was on {_lastRunDate.Date}.");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<RankTrackerDbContext>();
                    var seleniumRankChecker = scope.ServiceProvider.GetRequiredService<ISeleniumRankChecker>();

                    _logger.LogInformation("Scope created. Fetching website/keyword associations.");
                    try
                    {
                        var associations = await dbContext.WebsiteKeywords
                                               .Include(wk => wk.Website)
                                               .Include(wk => wk.Keyword)
                                               .Where(wk => wk.Website != null && wk.Keyword != null) // Ensure related entities exist
                                               .ToListAsync();

                        _logger.LogInformation($"Found {associations.Count} valid associations to check.");

                        foreach (var assoc in associations)
                        {
                            _logger.LogInformation($"Checking rank for URL: '{assoc.Website.Url}', Keyword: '{assoc.Keyword.Text}'.");
                            (int? rank, string notes) = (null, "Check not performed or failed.");
                            try
                            {
                                // Perform the rank check
                                (rank, notes) = await seleniumRankChecker.GetRankAsync(assoc.Website.Url, assoc.Keyword.Text);
                                _logger.LogInformation($"Rank check result for '{assoc.Website.Url}' / '{assoc.Keyword.Text}': Rank={rank?.ToString() ?? "N/A"}, Notes='{notes}'.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error during Selenium rank check for '{assoc.Website.Url}' / '{assoc.Keyword.Text}'.");
                                notes = $"Automated check error: {ex.Message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Unknown error"}";
                            }

                            var rankingLog = new RankingLog
                            {
                                WebsiteId = assoc.WebsiteId,
                                KeywordId = assoc.KeywordId,
                                Rank = rank,
                                SearchDate = DateTime.UtcNow,
                                Notes = notes
                            };
                            dbContext.RankingLogs.Add(rankingLog);
                        }

                        if (associations.Any()) {
                           await dbContext.SaveChangesAsync();
                           _logger.LogInformation("Finished performing daily rank checks and saved results.");
                        } else {
                            _logger.LogInformation("No associations found to process.");
                        }
                        _lastRunDate = now.Date; // Update last run date only after successful processing (or at least an attempt)
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred within scheduled rank check task execution.");
                    }
                }
                 _logger.LogInformation("Scheduled work finished for this run.");
            }
            else
            {
                _logger.LogInformation($"Not time to run daily checks yet or already ran for {now.Date}. Current time: {now.TimeOfDay}, Last run date: {_lastRunDate.Date}, Target time: {_runAtTimeOfDay}.");
            }
             // The timer will automatically reschedule for the next _checkInterval.
             // To make the *next day's* run precise, adjust timer here:
            var nextScheduledRun = DateTime.Today.Add(_runAtTimeOfDay);
            if (DateTime.Now >= nextScheduledRun) // If current time is already past today's target time
            {
                nextScheduledRun = nextScheduledRun.AddDays(1); // Aim for tomorrow
            }
            // If _lastRunDate is today, this means we've already run, so next run is definitely tomorrow.
            if (_lastRunDate.Date == DateTime.Today) {
                 nextScheduledRun = DateTime.Today.AddDays(1).Add(_runAtTimeOfDay);
            }


            TimeSpan delayUntilNextPreciseRun = nextScheduledRun - DateTime.Now;
            if (delayUntilNextPreciseRun < TimeSpan.Zero) delayUntilNextPreciseRun = TimeSpan.Zero; // Should not be negative

            // Set the timer to fire at the next precise _runAtTimeOfDay, or after _checkInterval, whichever is sooner,
            // to ensure we don't miss a check if the app restarts.
            // However, the logic in DoWorkAsync already ensures it only runs once per day at the right time.
            // So, just letting it tick at _checkInterval is simpler.
            // If precise timing after each run is desired: _timer.Change(delayUntilNextPreciseRun, _checkInterval);
            // For now, stick to fixed _checkInterval. The logic within DoWorkAsync determines if work is done.
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scheduled Rank Check Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
