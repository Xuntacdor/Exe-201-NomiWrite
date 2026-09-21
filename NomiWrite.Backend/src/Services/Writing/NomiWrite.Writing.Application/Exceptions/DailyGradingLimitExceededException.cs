namespace NomiWrite.Writing.Application.Exceptions;

public class DailyGradingLimitExceededException : Exception
{
    public DailyGradingLimitExceededException()
        : base("Daily free grading limit reached. Try again tomorrow or subscribe to VIP.") { }
}
