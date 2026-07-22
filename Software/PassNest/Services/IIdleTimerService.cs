using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Services
{
    public interface IIdleTimerService
    {
        event Action? TimedOut;
        void Start(TimeSpan? timeout);
        void Stop();
    }
}
