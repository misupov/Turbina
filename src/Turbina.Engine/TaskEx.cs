using System.Threading;
using System.Threading.Tasks;

namespace Turbina.Engine
{
    public static class TaskEx
    {
        public static Task<T> IgnoreCancellation<T>(this Task<T> task)
        {
            return task.ContinueWith(t => default(T), TaskContinuationOptions.OnlyOnCanceled);
        }

        public static Task IgnoreCancellation(this Task task)
        {
            return task.ContinueWith(t => {}, TaskContinuationOptions.OnlyOnCanceled);
        }
    }
}