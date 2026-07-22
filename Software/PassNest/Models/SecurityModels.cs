using Avalonia.Media;

namespace PassNest.Models
{
    public class SecurityModels
    {
        public int AccountId { get; }
        public string Initial { get; }
        public IBrush AvatarColor { get; }
        public string ServiceName { get;}
        public string IssueDescription { get; }
        public IBrush SeverityColor { get; }
        public SecurityModels(int accountId, string initial, string avatarColorHex, string serviceName, string issueDescription, string severityColorHex)
        {
            AccountId = accountId;
            Initial = initial;
            AvatarColor = new SolidColorBrush(Color.Parse(avatarColorHex));
            ServiceName = serviceName;
            IssueDescription = issueDescription;
            SeverityColor = new SolidColorBrush(Color.Parse(severityColorHex));
        }
    }
}
