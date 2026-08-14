using DataAccessLayer.UpdateCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.UpdateCheck
{
    public class UpdateChecker : IUpdateChecker
    {
        private readonly IUpdateSource updateSource;

        public UpdateChecker(IUpdateSource updateSource)
        {
            this.updateSource = updateSource;
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync(Version currentVersion)
        {
            var latestRelease = await updateSource.GetLatestReleaseAsync();
            if (latestRelease == null) return UpdateCheckResult.NoUpdate();

            var tag = latestRelease.TagName.TrimStart('v', 'V');
            if(!Version.TryParse(tag, out var latestVersion))
            {
                return UpdateCheckResult.NoUpdate();
            }

            return latestVersion > currentVersion
                ? UpdateCheckResult.Available(latestVersion.ToString(), latestRelease.Body, latestRelease.HtmlUrl)
                : UpdateCheckResult.NoUpdate();
        }
    }
}
