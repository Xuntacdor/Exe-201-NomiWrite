namespace NomiWrite.Admin.Application.Exceptions;

public class ContentReportNotFoundException : Exception
{
    public ContentReportNotFoundException()
        : base("Content report not found.")
    {
    }
}
