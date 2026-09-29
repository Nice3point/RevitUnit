using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents a command the test host sends to the Revit user interface application.
/// </summary>
[PublicAPI]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "command")]
[JsonDerivedType(typeof(RevitUiTestSchedule), "schedule")]
[JsonDerivedType(typeof(RevitUiTestExecution), "execute")]
[JsonDerivedType(typeof(RevitUiTestCancellation), "cancel")]
[JsonDerivedType(typeof(RevitUiTestSessionEnd), "end")]
internal abstract record RevitUiTestCommand
{
    /// <summary>
    ///     Parses a command from a message of the test host.
    /// </summary>
    /// <param name="message">The message to parse.</param>
    /// <returns>The parsed command.</returns>
    /// <exception cref="JsonException">The message holds no valid command.</exception>
    [Pure]
    public static RevitUiTestCommand Parse(string message)
    {
        return JsonSerializer.Deserialize(message, RevitUiMessageJsonContext.Default.RevitUiTestCommand) ?? throw new JsonException("The message holds no command.");
    }

    /// <summary>
    ///     Serializes the command into a message.
    /// </summary>
    /// <returns>The serialized command.</returns>
    [Pure]
    public string Serialize()
    {
        return JsonSerializer.Serialize(this, RevitUiMessageJsonContext.Default.RevitUiTestCommand);
    }
}
