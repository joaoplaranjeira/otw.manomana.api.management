namespace ManoMana.Application.Exceptions;

public abstract class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

public sealed class ValidationException(string code, string message) : AppException(code, message);
public sealed class UnauthorizedException(string code, string message) : AppException(code, message);
public sealed class ForbiddenException(string code, string message) : AppException(code, message);
public sealed class NotFoundException(string code, string message) : AppException(code, message);
public sealed class ConflictException(string code, string message) : AppException(code, message);
