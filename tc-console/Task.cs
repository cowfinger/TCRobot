﻿namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class Task
    {
        private readonly IEnumerable<Task> taskGroup;

        private readonly Thread taskThread;

        public Task(IEnumerable<Task> tasks)
        {
            this.taskGroup = tasks;
        }

        public Task(Action action)
        {
            this.taskThread = new Thread(new ThreadStart(action));
            this.taskThread.Start();
        }

        public static Task Run(Action action)
        {
            return new Task(action);
        }

        public static void WaitAll(IEnumerable<Task> tasks)
        {
            var taskList = tasks.ToList();
            while (taskList.Any())
            {
                var toBeRemoveList = taskList.Where(task => task.Wait(100)).ToList();

                foreach (var task in toBeRemoveList)
                {
                    taskList.Remove(task);
                }
            }
        }

        public static bool WaitAll(IEnumerable<Task> tasks, int miliseconds)
        {
            var timer = DateTime.Now;

            var taskList = tasks.ToList();
            while (taskList.Any())
            {
                foreach (var task in taskList)
                {
                    if (task.Wait(miliseconds))
                    {
                        taskList.Remove(task);
                    }

                    var diff = DateTime.Now - timer;
                    if (diff.TotalMilliseconds >= miliseconds)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public Task Then(Action action)
        {
            return new Task(
                () =>
                    {
                        this.Wait();
                        action();
                    });
        }

        public bool Wait(int miliseconds)
        {
            if (this.taskThread != null)
            {
                return this.taskThread.Join(miliseconds);
            }

            if (this.taskGroup != null)
            {
                return WaitAll(this.taskGroup, miliseconds);
            }

            throw new NotImplementedException("Neither Task Thread nor Sub Tasks are valid.");
        }

        public void Wait()
        {
            if (this.taskThread != null)
            {
                this.taskThread.Join();
            }

            if (this.taskGroup != null)
            {
                WaitAll(this.taskGroup);
            }
        }
    }

    internal static class Parallel
    {
        public static IEnumerable<Task> DispatchTask<T>(IEnumerable<T> tasks, Action<T> action)
        {
            return tasks.Select(taskData => new Task(() => action(taskData)));
        }

        public static Task Dispatch<T>(IEnumerable<T> tasks, Action<T> action)
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