using System.Net;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Exceptions;

public class TwiHighApiRequestException : TwiHighException
{
    public HttpStatusCode StatusCode { get; private set; } = HttpStatusCode.InternalServerError;

    public TwiHighApiRequestException(HttpRequestException? httpRequestException = null)
    {
        StatusCode = httpRequestException?.StatusCode ?? HttpStatusCode.InternalServerError;
    }

    public TwiHighApiRequestException(string? message) : base(message)
    {
    }

    public TwiHighApiRequestException(string? message, HttpRequestException? httpRequestException) : base(message, httpRequestException)
    {
        StatusCode = httpRequestException?.StatusCode ?? HttpStatusCode.InternalServerError;
    }

    public TwiHighApiRequestException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

}
