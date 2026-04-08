﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace StoreSyncFront.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username;

    public HomeViewModel(string username)
    {
        _username = username;
    }
}