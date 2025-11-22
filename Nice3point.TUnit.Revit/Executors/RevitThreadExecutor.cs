using System.Diagnostics.CodeAnalysis;
using System.Windows.Threading;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
/// Executes a provided asynchronous action on the Revit thread dispatcher.
/// Ensures that the action is safely executed within the context of Revit's single-threaded API.
/// </summary>
public sealed class RevitThreadExecutor : GenericAbstractExecutor
{
    private static Dispatcher? _dispatcher;

    /// <summary>
    /// Executes an asynchronous action on the Revit thread dispatcher.
    /// Ensures that the action is executed within the context of Revit's single-threaded API.
    /// </summary>
    /// <param name="action">The asynchronous action to execute on the Revit thread dispatcher.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous execution operation.</returns>
    protected override async ValueTask ExecuteAsync(Func<ValueTask> action)
    {
        _dispatcher ??= RunRevitDispatcher();
        await _dispatcher.InvokeAsync(() =>
        {
            var valueTask = action();
            if (!valueTask.IsCompletedSuccessfully)
            {
                valueTask.AsTask().GetAwaiter().GetResult();
            }
        });

#if NET
        await ValueTask.CompletedTask;
#else
        await new ValueTask();
#endif
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private static Dispatcher RunRevitDispatcher()
    {
        using var dispatcherReadyEvent = new ManualResetEventSlim(false);
        var uiThread = new Thread(() =>
        {
            //Create a new Dispatcher
            _ = Dispatcher.CurrentDispatcher;
            dispatcherReadyEvent.Set();

            //Borrow a thread
            Dispatcher.Run();
        });

        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.IsBackground = true;
        uiThread.Start();

        dispatcherReadyEvent.Wait();
        return Dispatcher.FromThread(uiThread)!;
    }
}