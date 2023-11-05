// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Model;
public partial class SharedModels : ObservableObject
{
    [PrimaryKey, AutoIncrement, Column("Id")]
    public int Id { get; set; }
    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _description;
    [ObservableProperty]
    private string _link;
    [ObservableProperty]
    private string _image;
    [ObservableProperty]
    private string _url;
    [ObservableProperty]
    private DateTime _pubDate;
    [ObservableProperty]
    private bool _download;
    [ObservableProperty]
    private bool _isDownloaded;
    [ObservableProperty]
    private bool _isNotDownloaded;
}
