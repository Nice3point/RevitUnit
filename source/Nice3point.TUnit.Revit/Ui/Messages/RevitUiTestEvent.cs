using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents an event the Revit user interface application sends about one UI test.
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "event")]
[JsonDerivedType(typeof(RevitUiTestPending), "pending")]
[JsonDerivedType(typeof(RevitUiTestResult), "result")]
internal abstract record RevitUiTestEvent
{
    /// <summary>
    ///     Gets the identifier of the test the event belongs to.
    /// </summary>
    public required string TestId { get; init; }

    /// <summary>
    ///     Parses an event from a message of the Revit user interface application.
    /// </summary>
    /// <param name="message">The message to parse.</param>
    /// <returns>The parsed event.</returns>
    /// <exception cref="JsonException">The message holds no valid event.</exception>
    [Pure]
    public static RevitUiTestEvent Parse(string message)
    {
        return JsonSerializer.Deserialize(message, RevitUiMessageJsonContext.Default.RevitUiTestEvent) ?? throw new JsonException("The message holds no test event.");
    }

    /// <summary>
    ///     Serializes the event into a message.
    /// </summary>
    /// <returns>The serialized event.</returns>
    [Pure]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RevitUiMessageJsonContext.Default.RevitUiTestEvent);
    }
}
