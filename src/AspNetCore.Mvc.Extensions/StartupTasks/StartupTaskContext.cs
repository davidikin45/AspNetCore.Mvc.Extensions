using System.Threading;

namespace AspNetCore.Mvc.Extensions.StartupTasks
{
    public class StartupTaskContext
    {
        private int _outstandingTaskCount = 0;

        public bool RegisterTask()
        {
            Interlocked.Increment(ref _outstandingTaskCount);

            return true;
        }

        public void MarkTaskAsComplete()
        {
            Interlocked.Decrement(ref _outstandingTaskCount);
        }

        public bool IsComplete => _outstandingTaskCount == 0;
    }
}
