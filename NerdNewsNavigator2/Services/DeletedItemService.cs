// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace NerdNewsNavigator2.Services;
public partial class DeletedItemService : ObservableObject
{
    [ObservableProperty]
    private Download _item;
    public EventHandler<DeletedItemEventArgs> DeletedItem { get; set; }
    public DeletedItemService()
    {
        _item = new();
    }
    public void Add(Download download)
    {
        var args = new DeletedItemEventArgs
        {
            Item = download,
        };
        OnDeletedItem(args);
    }

    private void OnDeletedItem(DeletedItemEventArgs args)
    {
        DeletedItem?.Invoke(this, args);
    }
}
