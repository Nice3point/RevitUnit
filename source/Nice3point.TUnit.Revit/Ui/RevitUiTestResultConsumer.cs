using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;
using Nice3point.TUnit.Revit.Ui.Messages;

namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents the data consumer that sends the final state of every UI test to the test host.
/// </summary>
/// <param name="runtime">The runtime of the UI tests inside the Revit user interface application.</param>
/// <remarks>
///     The test platform reports every test once in its final state, whether a hook, the constructor, a data source, or the body ended it.
///     The final state holds the output the test wrote.
///     The consumer sends a final state once the body of the test finishes, and a body that outlives the timeout of its test delays the state of every later test.
/// </remarks>
internal sealed class RevitUiTestResultConsumer(RevitUiApplicationRuntime runtime) : IDataConsumer
{
    /// <inheritdoc />
    public Type[] DataTypesConsumed { get; } = [typeof(TestNodeUpdateMessage)];

    /// <inheritdoc />
    public string Uid => nameof(RevitUiTestResultConsumer);

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string DisplayName => "Revit UI test result consumer";

    /// <inheritdoc />
    public string Description => "Sends the final state of every UI test to the test host.";

    /// <inheritdoc />
    public Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        var testNode = ((TestNodeUpdateMessage)value).TestNode;
        if (RevitUiTestResult.FromTestNode(testNode) is not { } result)
        {
            return Task.CompletedTask;
        }

        return runtime.ReportAsync(result);
    }
}
