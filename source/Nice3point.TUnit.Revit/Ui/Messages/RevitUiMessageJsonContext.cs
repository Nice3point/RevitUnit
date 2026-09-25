using System.Text.Json.Serialization;

namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Provides the source-generated serialization context of the UI test messages.
/// </summary>
[JsonSerializable(typeof(RevitUiTestResult))]
internal sealed partial class RevitUiMessageJsonContext : JsonSerializerContext;
