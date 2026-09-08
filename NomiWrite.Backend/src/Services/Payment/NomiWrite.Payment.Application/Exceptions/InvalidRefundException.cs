namespace NomiWrite.Payment.Application.Exceptions;

public class InvalidRefundException : Exception
{
    public InvalidRefundException(string message)
        : base(message)
    {
    }

    public InvalidRefundException(string message, int statusCode)
        : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; set; } = 400;
}
