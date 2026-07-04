namespace BlazorApp1.Services.App;

public class SubmitGuard
{
    public bool IsBusy { get; private set; }

    public async Task RunAsync(Func<Task> action)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await action();
        }
        finally
        {
            IsBusy = false;
        }
    }
}
