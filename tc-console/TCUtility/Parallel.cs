namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Parallel
    {
        class ItemPool<T>
        {
            private readonly List<T> itemList;

            private int index;

            public ItemPool(IEnumerable<T> items)
            {
                this.itemList = items.ToList();
            }

            public bool Pop(out T item)
            {
                lock (this)
                {
                    if (this.index < this.itemList.Count)
                    {
                        item = this.itemList[this.index];
                        ++this.index;
                        return true;
                    }
                    item = this.itemList[0];
                    return false;
                }
            }
        }

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

        public static void ForEach<T>(IEnumerable<T> tasks, int concurrency, Action<T> action)
        {
            var pool = new ItemPool<T>(tasks);
            var poolList = new List<ItemPool<T>>();
            for (int i = 0; i < concurrency; i++)
            {
                poolList.Add(pool);
            }

            Dispatch(poolList, poolItem =>
            {
                T item;
                while (poolItem.Pop(out item))
                {
                    action(item);
                }
            }).Wait();
        }
    }
}