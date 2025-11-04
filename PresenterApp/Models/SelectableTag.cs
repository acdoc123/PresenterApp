// File: Models/SelectableTag.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace PresenterApp.Models
{
    public partial class SelectableTag : ObservableObject
    {
        [ObservableProperty]
        bool isSelected;

        public Tag Tag { get; set; }
    }
}