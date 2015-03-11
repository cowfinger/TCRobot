namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Parallel
    {
        public static IEnumerable<Task> DispatchTask<T>(IEnumerable<T> tasks, Action<T> action)
        {
            return tasks.Select(taskData => new Task(() => action(taskData)));
        }

        public static IEnumerable<Task> DispatchTask<T, U>(IEnumerable<T> tasks, Func<T, U> action)
        {
            return tasks.Select(taskData => new Task(() => action(taskData)));
        }

        public static Task Dispatch<T>(IEnumerable<T> tasks, Action<T> action)
        {
            var batchTask = DispatchTask(tasks, action).ToList(); // ToList() to ensure Tasks are all created.
            return new Task(batchTask);
        }

        public static Task Dispatch<T, U>(IEnumerable<T> tasks, Func<T, U> action)
        {
            var batchTask = DispatchTask(tasks, action).ToList(); // ToList() to ensure Tasks are all created.
            return new Task(batchTask);
        }

        public static void ForEach<T>(IEnumerable<T> tasks, Action<T> action)
        {
            Dispatch(tasks, action).Wait();
        }
    }
}