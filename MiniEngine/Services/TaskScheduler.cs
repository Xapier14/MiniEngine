using System;
using System.Collections.Generic;
using System.Diagnostics;
using MiniEngine.Utility;

namespace MiniEngine
{
    public class TaskScheduler
    {
        private readonly PriorityQueue<(Action Callback, long RecurringPeriod), long> _scheduledTasks = new();
        public double TaskDurationThreshold { get; set; } = 3.0;
        public double UpdateDurationThreshold { get; set; } = 6.0;

        public void UpdateAndExecute()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var totalElapsed = 0.0;
            while (_scheduledTasks.TryPeek(out var scheduledTask, out var scheduledTicks))
            {
                var ticksNow = DateTime.Now.Ticks;
                if (ticksNow < scheduledTicks)
                    break;
                // if this update duration is already greater than threshold,
                // break out of the loop and resume on next update.
                if (totalElapsed > UpdateDurationThreshold)
                    break;

                scheduledTask.Callback();
                _scheduledTasks.Dequeue();

                // check individual task if it exceeds threshold. if so, print warning
                if (stopwatch.Elapsed.TotalMilliseconds > TaskDurationThreshold)
                {
                    LoggingService.Warn("[TaskScheduler] Scheduled task took {0}ms to complete. (Threshold: {1}ms)", stopwatch.Elapsed.TotalMilliseconds, TaskDurationThreshold);
                    LoggingService.Warn("[TaskScheduler] Use AsyncDispatcher for long running tasks.");
                }
                totalElapsed += stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();

                if (scheduledTask.RecurringPeriod <= 0)
                    continue;

                var recurringPeriodInTicks = TimeSpan.FromMilliseconds(scheduledTask.RecurringPeriod).Ticks;
                _scheduledTasks.Enqueue(scheduledTask, ticksNow + recurringPeriodInTicks);
                totalElapsed += stopwatch.Elapsed.TotalMilliseconds;
                stopwatch.Restart();
            }
            stopwatch.Stop();
        }

        public void Schedule(DateTime scheduledDateTime, Action callback, long recurringPeriod = 0)
            => _scheduledTasks.Enqueue((callback, recurringPeriod), scheduledDateTime.Ticks);

        public void ScheduleIn(long durationFromNow, Action callback, long recurringPeriod = 0)
            => _scheduledTasks.Enqueue(
                (callback, recurringPeriod),
                DateTime.Now.Ticks + TimeSpan.FromMilliseconds(durationFromNow).Ticks
            );

        public void ScheduleNow(Action callback, long recurringPeriod = 0)
            => ScheduleIn(0, callback, recurringPeriod);
    }
}
