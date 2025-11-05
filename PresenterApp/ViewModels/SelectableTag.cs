// File: Models/SelectableTag.cs
using CommunityToolkit.Mvvm.ComponentModel;
using PresenterApp.Models;

namespace PresenterApp.ViewModels
{
    public partial class SelectableTag : ObservableObject
    {
        [ObservableProperty]
        bool isSelected;

        public Tag Tag { get; set; }
    }
}