using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Collections;
using PassNest.Models;
using BusinessLogicLayer.PasswordAudit;
using BusinessLogicLayer.PasswordGeneration;
using BusinessLogicLayer.AccountManagement;
using System.ComponentModel;
using System.Linq;

namespace PassNest.ViewModels
{
    public partial class SecurityViewModel : ViewModelBase
    {
        private const double RingRadius = 64;
        private const double RingStrokeThickness = 12;

        private readonly IPasswordAuditor passwordAuditor;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly IAccountStore accountStore;

        [ObservableProperty]
        private int score;

        [ObservableProperty]
        private int weakCount;

        [ObservableProperty]
        private int mediumCount;

        [ObservableProperty]
        private int strongCount;

        public string ScoreHeading => $"Stanje trezora: {ScoreStatusLabel}";

        public string ScoreStatusLabel => Score switch
        {
            >= 70 => "Dobro",
            >= 40 => "Srednje",
            _ => "Loše"
        };

        public IBrush ScoreColor => Score switch
        {
            >= 70 => new SolidColorBrush(Color.Parse("#2AA26A")),
            >= 40 => new SolidColorBrush(Color.Parse("#E0952E")),
            _ => new SolidColorBrush(Color.Parse("#D6503C"))
        };

        public AvaloniaList<double> ScoreDashArray
        {
            get
            {
                double circumferenceUnits = (2 * Math.PI * RingRadius) / RingStrokeThickness;
                double onUnits = circumferenceUnits * (Score / 100.00);
                double offUnits = circumferenceUnits - onUnits;
                return new AvaloniaList<double> { onUnits, offUnits };
            }
        }

        public ObservableCollection<SecurityModels> Issues { get; } = new();

        public event Action<int>? AccountFixRequested;

        public SecurityViewModel(IPasswordAuditor passwordAuditor, IAccountStore accountStore, IPasswordGenerator passwordGenerator)
        {
            this.passwordAuditor = passwordAuditor;
            this.accountStore = accountStore;
            this.passwordGenerator = passwordGenerator;

            RunAudit();
        }

        partial void OnScoreChanged(int value)
        {
            OnPropertyChanged(nameof(ScoreHeading));
            OnPropertyChanged(nameof(ScoreStatusLabel));
            OnPropertyChanged(nameof(ScoreColor));
            OnPropertyChanged(nameof(ScoreDashArray));
        }

        private void RunAudit()
        {
            var credentials = accountStore.GetAllCredentials().ToList();

            var weak = 0;
            var medium = 0;
            var strong = 0;

            foreach(var credential in credentials)
            {
                switch (passwordGenerator.EvaluateStrength(credential.Password))
                {
                    case PasswordStrengthLevel.VrloSlaba:
                    case PasswordStrengthLevel.Slaba:
                        weak++;
                        break;
                    case PasswordStrengthLevel.Srednja:
                        medium++;
                        break;
                    default:
                        strong++;
                        break;
                }
            }

            WeakCount = weak;
            MediumCount = medium;
            StrongCount = strong;

            var total = credentials.Count;
            Score = total == 0 ? 100 : (int)Math.Round(100.0 * (strong + medium * 0.5) / total);

            Issues.Clear();
            foreach(var entry in passwordAuditor.AuditPasswords())
            {
                var severityColorHex = entry.Reason == "Vrlo slaba lozinka" ? "#D6503C" : "#E0952E";

                Issues.Add(new SecurityModels(
                    entry.AccountId,
                    AvatarColorPicker.GetInitial(entry.ServiceName),
                    AvatarColorPicker.GetColor(entry.ServiceName),
                    entry.ServiceName,
                    entry.Reason,
                    severityColorHex
                ));
            }
        }

        [RelayCommand]
        private void FixIssue(SecurityModels issue)
        {
            AccountFixRequested?.Invoke(issue.AccountId);
        }
    }
}
