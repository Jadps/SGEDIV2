namespace BACKSGEDI.Domain.Common;

public enum ErrorType
{
    Failure = 0,
    Unexpected = 1,
    Validation = 2,
    Conflict = 3,
    NotFound = 4,
    Unauthorized = 5,
    Forbidden = 6
}

public record Error
{
    public string Code { get; }
    public string Description { get; }
    public ErrorType Type { get; }

    private Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public static Error Failure(string code, string description) => new(code, description, ErrorType.Failure);
    public static Error Unexpected(string code, string description) => new(code, description, ErrorType.Unexpected);
    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);
    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);
    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);
    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}
