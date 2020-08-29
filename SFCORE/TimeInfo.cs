using System.Diagnostics;

namespace SFCORE
{
    internal class TimeInfo : ITimeInfo
    {
        public float CurrentDt { get; set; }
        public float CurrentTime => (float)_watch.Elapsed.TotalSeconds;
        public int CurrentFrame { get; private set; }
        private readonly Stopwatch _watch = new Stopwatch();
        private double _previousTick;

        public TimeInfo()
        {
            _watch.Start();
            _previousTick = _watch.Elapsed.TotalSeconds;
        }

        public void Tick()
        {
            CurrentFrame++;
            var currentTick = _watch.Elapsed.TotalSeconds;
            CurrentDt = (float)(currentTick - _previousTick);
            _previousTick = currentTick;
        }
    }
}