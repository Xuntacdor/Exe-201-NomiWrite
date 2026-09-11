namespace NomiWrite.Admin.Application.Exceptions;

public class DuplicateContentReportException : Exception
{
    public DuplicateContentReportException()
        : base("A pending report for this content already exists.")
    {
    }
}
