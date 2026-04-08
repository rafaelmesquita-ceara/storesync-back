﻿using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace StoreSyncFront.ViewModels;

public partial class TabItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    private object _content;

    public bool IsClosable { get; }

    public ICommand CloseCommand { get; }

    public TabItemViewModel(string header, object content, bool isClosable, Action<TabItemViewModel> closeAction)
    {
        _header = header;
        _content = content;
        IsClosable = isClosable;
        CloseCommand = new RelayCommand(() => closeAction(this));
    }
}