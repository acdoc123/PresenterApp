using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PresenterApp.Models;

public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]

    bool isBusy;

    [ObservableProperty]
    string title;

    public bool IsNotBusy => !isBusy;
}