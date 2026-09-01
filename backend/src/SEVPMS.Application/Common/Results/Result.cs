namespace SEVPMS.Application.Common.Results;

public sealed class Result
{
    public bool Succeeded { get; private init; }
    public string? Error { get; private init; }

    public static Result Success() => new() { Succeeded = true };
    public static Result Failure(string error) => new() { Succeeded = false, Error = error };
}
