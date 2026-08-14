using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.UpdateCheck
{
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable { get; }
        public string LatestVersion { get; }
        public string ReleaseNotes { get; }
        public string ReleaseUrl { get; }

        public UpdateCheckResult(bool isUpdateAvailable, string latestVersion, string releaseNotes, string releaseUrl)
        {
            IsUpdateAvailable = isUpdateAvailable;
            LatestVersion = latestVersion;
            ReleaseNotes = releaseNotes;
            ReleaseUrl = releaseUrl;
        }

        public static UpdateCheckResult NoUpdate() => new(false, string.Empty, string.Empty, string.Empty);

        public static UpdateCheckResult Available(string latestVersion, string releaseNotes, string releaseUrl) => new(true, latestVersion, releaseNotes, releaseUrl);
    }
}
