using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.UpdateCheck
{
    public interface IUpdateSource
    {
        Task<GitHubReleaseInfo?> GetLatestReleaseAsync();
    }
}
