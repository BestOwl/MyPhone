using GoodTimeStudio.MyPhone.Utilities;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace MyPhone.UnitTest.Utilities
{
    public class TestDynamicTimer1
    {
        [Fact]
        public async Task TestSubsequentZeroMilliseconds()
        {
            var schedules = new[]
            {
                new DynamicTimerSchedule(new TimeSpan(0, 0, 0, 1, 0), 2),
                new DynamicTimerSchedule(new TimeSpan(0, 0, 0, 1, 0), 2),

            };
            DynamicTimer timer = new DynamicTimer(schedules);
            int count = 0;
            timer.Elapsed += (object? sender, DynamicTimerElapsedEventArgs e) =>
            {
                count++;
            };

            timer.Start();
            await Task.Delay(7000);
            timer.Stop();
            Assert.Equal(4, count);
        }
    }

    public class TestDynamicTimer2
    {
        [Fact]
        public async Task TestOneLongInterval()
        {
            var schedules = new[]
            {
                new DynamicTimerSchedule(TimeSpan.FromSeconds(2), 5),
            };
            DynamicTimer timer = new DynamicTimer(schedules);
            int count = 0;
            timer.Elapsed += (object? sender, DynamicTimerElapsedEventArgs e) =>
            {
                count++;
            };

            timer.Start();
            await Task.Delay(12000);
            timer.Stop();
            Assert.Equal(5, count);
        }
    }

    public class TestDynamicTimer3
    {
        private readonly ITestOutputHelper _output;

        public TestDynamicTimer3(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestOneIndefinite()
        {
            var schedules = new[]
            {
                new DynamicTimerSchedule(TimeSpan.FromSeconds(0.1), 0),
            };
            DynamicTimer timer = new DynamicTimer(schedules);
            int count = 0;

            timer.Elapsed += (object? sender, DynamicTimerElapsedEventArgs e) =>
            {
                count++;
            };

            timer.Start();
            await Task.Delay(4000);
            timer.Stop();
            Assert.True(count > 30);
        }

        [Fact]
        public async Task TestThreeDifferentInterval()
        {
            // 2.4 seconds in total
            var schedules = new[]
            {
                new DynamicTimerSchedule(TimeSpan.FromSeconds(0.1), 4),
                new DynamicTimerSchedule(TimeSpan.FromSeconds(0.2), 4),
                new DynamicTimerSchedule(TimeSpan.FromSeconds(0.3), 4),
            };
            DynamicTimer timer = new DynamicTimer(schedules);
            int count = 0;
            DateTime previousTime = DateTime.MinValue;

            timer.Elapsed += (object? sender, DynamicTimerElapsedEventArgs e) =>
            {
                count++;
                if (count == 1)
                {
                    previousTime = e.SignalTime;
                    return;
                }

                TimeSpan interval = e.SignalTime - previousTime;
                _output.WriteLine($"Interval: {interval}");
                if (count <= 4)
                {
                    Assert.InRange(interval.TotalSeconds, 0.075, 0.125);
                }
                else if (count <= 8)
                {
                    Assert.InRange(interval.TotalSeconds, 0.175, 0.225);
                }
                else
                {
                    Assert.InRange(interval.TotalSeconds, 0.275, 0.325);
                }
                previousTime = e.SignalTime;
            };

            timer.Start();
            await Task.Delay(4000);
            Assert.False(timer.Enabled);
            timer.Dispose();
            Assert.Equal(12, count);
        }
    }
}
