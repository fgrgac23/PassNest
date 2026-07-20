using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassNest.Models
{
    public class BreadcrumbItem
    {
        public string Label { get; }
        public IRelayCommand? NavigateCommand { get; }
        public bool ShowSeparator { get; set; }

        public BreadcrumbItem(string label, Action? navigate = null)
        {
            Label = label;
            NavigateCommand = navigate is null ? null : new RelayCommand(navigate);
        }
    }
}
