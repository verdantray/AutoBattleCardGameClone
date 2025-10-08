using System;
using System.Threading.Tasks;

namespace ProjectABC.Utils
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task, Action<Exception> onException = null, bool ignoreCancellation = true, bool continueOnCapturedContext = false)
        {
            if (task.IsCompleted)
            {
                ObserveCompletion(task, onException, ignoreCancellation);
                return;
            }

            _ = ForgetAwaited(task, onException, ignoreCancellation, continueOnCapturedContext);
        }

        private static async Task ForgetAwaited(Task task, Action<Exception> onException, bool ignoreCancellation, bool continueOnCapturedContext)
        {
            try
            {
                await task.ConfigureAwait(continueOnCapturedContext);
            }
            catch (OperationCanceledException) when (ignoreCancellation)
            {

            }
            catch (Exception e)
            {
                onException?.Invoke(e);
            }
        }

        private static void ObserveCompletion(Task completed, Action<Exception> onException, bool ignoreCancellation)
        {
            if (completed.IsFaulted)
            {
                var exception = completed.Exception!.GetBaseException();
                onException?.Invoke(exception);
            }
            else if (completed.IsCanceled && !ignoreCancellation)
            {
                onException?.Invoke(new TaskCanceledException(completed));
            }
        }
    }
}
