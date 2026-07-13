using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Collections;
using PassNest.Models;

namespace PassNest.ViewModels
{
    public partial class SecurityViewModel : ViewModelBase
    {
        private const double RingRadius = 64;
        private const double RingStrokeThickness = 12;

        [ObservableProperty]
        private int score = 72;

        [ObservableProperty]
        private int weakCount = 3;

        [ObservableProperty]
        private int mediumCount = 5;

        [ObservableProperty]
        private int strongCount = 19;

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

        public ObservableCollection<SecurityModels> Issues { get; } = new()
        {
            new SecurityModels("S", "#2AA26A", "Spotify", "Vrlo slaba — 6 znakova, bez brojeva", "#D6503C"),
            new SecurityModels("N", "#D6503C", "Netflix", "Slaba - 5 broja", "#E0952E"),
            new SecurityModels("L", "#0F7B8A", "LinkedIn", "Srednja — 5 znaka i 1 broj", "#E0952E")
        };

        partial void OnScoreChanged(int value)
        {
            OnPropertyChanged(nameof(ScoreHeading));
            OnPropertyChanged(nameof(ScoreStatusLabel));
            OnPropertyChanged(nameof(ScoreColor));
            OnPropertyChanged(nameof(ScoreDashArray));
        }

        [RelayCommand]
        private void FixIssue(SecurityModels issue)
        {
        }
    }
}
