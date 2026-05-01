namespace BFF.Shared.Exceptions;

public class DownstreamServiceException : Exception
{
    public string ServiceName { get; }
    public int? StatusCode { get; }

    public DownstreamServiceException(string serviceName, string message, int? statusCode = null)
        : base($"Service '{serviceName}' failed: {message}")
    {
        ServiceName = serviceName;
        StatusCode = statusCode;
    }
}
