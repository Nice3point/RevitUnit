using System.Text.Json;

namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the result of one UI test.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestResult
{
    /// <summary>
    ///     Gets the identifier of the test the result belongs to.
    /// </summary>
    public required string TestId { get; init; }

    /// <summary>
    ///     Gets the final state of the test.
    /// </summary>
    public required RevitUiTestStatus Status { get; init; }

    /// <summary>
    ///     Gets the failure or skip message, or <see langword="null" /> when the test passed.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    ///     Gets the stack trace of a failure, or <see langword="null" /> when none applies.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    ///     Parses a result from a message of the Revit user interface application.
    /// </summary>
    /// <param name="message">The message to parse.</param>
    /// <returns>The parsed result.</returns>
    /// <exception cref="JsonException">The message holds no valid result.</exception>
    [Pure]
    public static RevitUiTestResult Parse(string message)
    {
        return JsonSerializer.Deserialize(message, RevitUiMessageJsonContext.Default.RevitUiTestResult) ?? throw new JsonException("The message holds no test result.");
    }

    /// <summary>
    ///     Serializes the result into a message.
    /// </summary>
    /// <returns>The serialized result.</returns>
    [Pure]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RevitUiMessageJsonContext.Default.RevitUiTestResult);
    }
}
