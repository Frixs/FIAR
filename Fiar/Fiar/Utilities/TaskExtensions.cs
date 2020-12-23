using System;
using System.Threading;
using System.Threading.Tasks;

namespace Fiar
{
    /// <summary>
    /// Task extensions
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Run the task with additional timeout check
        /// </summary>
        /// <typeparam name="TResult">Return type of the task</typeparam>
        /// <typeparam name="TException">Return exception type</typeparam>
        /// <param name="task">The task</param>
        /// <param name="timeout">Timeout</param>
        /// <returns>Type of the task or TException</returns>
        public static async Task<TResult> TimeoutAfter<TResult, TException>(this Task<TResult> task, TimeSpan timeout)
            where TException : Exception, new()
        {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource())
            {

                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task)
                {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                }
                else
                {
                    throw new TException();
                }
            }
        }
    }
}
