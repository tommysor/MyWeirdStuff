namespace MyWeirdStuff.ApiService.Features.SharedFeature.Exceptions;

public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {}
}
