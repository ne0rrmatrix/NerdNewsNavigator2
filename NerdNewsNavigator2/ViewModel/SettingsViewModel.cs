﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    public ObservableCollection<JsonData> Files { get; set; } = new();
    public FileService File { get; set; }
    public SettingsViewModel(FileService file)
    {
        this.File = file;
        var list = File.GetData();
        foreach (var item in list)
        {
            Files.Add(item);
        }
        OnPropertyChanged(nameof(Files));
    }

    [RelayCommand]
    public Task Tap(int id)
    {
        Debug.WriteLine($"Id: {id}");
        File.DeleteItem(id);
        Files.Clear();
        var list = File.GetData();

        foreach (var item in list)
        {
            Files.Add(item);
        }
        OnPropertyChanged(nameof(Files));
        return Task.CompletedTask;
    }
}
