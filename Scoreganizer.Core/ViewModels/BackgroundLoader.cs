using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;

namespace Lomont.Scoreganizer.Core.ViewModels
{
    /// <summary>
    /// Batches work up in a loader that loads items,
    /// and every so often, calls an action on the UI thread
    /// on each of the loaded items
    /// </summary>
    /// <typeparam name="T"></typeparam>
    static class BackgroundLoader<T>
    {
        static void ExceptionCalled(Exception ex)
        {
            Trace.TraceError($"EXCEPTION: {ex}");
        }
        public static void LoadItemsAsync(IEnumerable<T> items, Action<T> itemAction, int groupSize = 10)
        {
            void Publish(Queue<T> files)
            {
                try
                {
                    while (files.TryDequeue(out var fd))
                        PerformUiAction(() => itemAction(fd));
                }
                catch (Exception ex)
                {
                    ExceptionCalled(ex);
                    throw;
                }
            }

            Task.Run(() =>
            {
                try
                {
                    var queue = new Queue<T>();
                    foreach (var item in items)
                    {
                        queue.Enqueue(item);
                        if (queue.Count >= groupSize)
                            Publish(queue);
                    }

                    Publish(queue);
                }
                catch (Exception ex)
                {
                    ExceptionCalled(ex);
                    throw;
                }
            });
        }
        public static async void PerformUiAction(Action action)
        {
            var mainThreadAsyncDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();
            await mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(action);
        }
    }
}
