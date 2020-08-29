namespace SFCORE
{
    public interface ITimeInfo
    {
        float CurrentDt { get; }
        float CurrentTime { get; }
        int CurrentFrame { get; }
    }
}