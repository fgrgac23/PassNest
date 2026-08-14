using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.UpdateCheck
{
    public interface IUpdateChecker
    {
        Task<UpdateCheckResult> CheckForUpdatesAsync(Version currentVersion);
    }
}
