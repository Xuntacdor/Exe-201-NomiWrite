namespace NomiWrite.Writing.Application.Exceptions;

public class MonthlyGradingLimitExceededException : Exception
{
    public MonthlyGradingLimitExceededException()
        : base("Your 5 free essays for this month have been used. Try again next month or upgrade to Premium.") { }
}
