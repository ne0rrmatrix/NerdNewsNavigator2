// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Foundation;
using Microsoft.Maui.Handlers;
using SQLitePCL;
using UIKit;

namespace NerdNewsNavigator2;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    // protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    // Next line is for SqlLite
    protected override MauiApp CreateMauiApp()
    {
        raw.SetProvider(new SQLite3Provider_sqlite3());
        return MauiProgram.CreateMauiApp();
    }

    // iOS Bug fix START

    public AppDelegate() : base()
    {
        // Update the Html attributed string for any of those property changes
        LabelHandler.Mapper.AppendToMapping(nameof(Label.Text), UpdateHtmlText);
        LabelHandler.Mapper.AppendToMapping(nameof(Label.FontFamily), UpdateHtmlText);
        LabelHandler.Mapper.AppendToMapping(nameof(Label.FontSize), UpdateHtmlText);
        LabelHandler.Mapper.AppendToMapping(nameof(Label.TextType), UpdateHtmlText);
    }

    void UpdateHtmlText(ILabelHandler handler, ILabel label)
    {
        var l = (Label)label;
        // Ignore non Html labels
        if (l.TextType != TextType.Html)
        {
            return;
        }
        var font = l.ToFont();
        var registrar = handler.MauiContext.Services.GetService<IFontRegistrar>();
        var fontName = CleanseFontName(font.Family, registrar);
        var fontSize = l.FontSize != 0 ? l.FontSize : UIFont.SystemFontSize;
        var text = l.Text != null ? l.Text : "";

        var attr = new NSAttributedStringDocumentAttributes
        {
            DocumentType = NSDocumentType.HTML,
            StringEncoding = NSStringEncoding.UTF8,
        };
        NSError nsError = null;
        // Create a new Attributed string with the HTML wrapped in a span with CSS setting the correct
        // font
        var r = $"<span style=\"font-family: '{fontName}'; font-size: {fontSize};\">{text}</span>";
        handler.PlatformView.AttributedText = new NSAttributedString(r, attr, ref nsError);
    }

    // Taken straight from src/Core/src/Fonts/FontManager.iOS.cs
#nullable enable
    string? CleanseFontName(string fontName, IFontRegistrar _fontRegistrar)
    {
        // First check Alias
        if (_fontRegistrar.GetFont(fontName) is string fontPostScriptName)
            return fontPostScriptName;

        var fontFile = FontFile.FromString(fontName);

        if (!string.IsNullOrWhiteSpace(fontFile.Extension))
        {
            if (_fontRegistrar.GetFont(fontFile.FileNameWithExtension()) is string filePath)
                return filePath ?? fontFile.PostScriptName;
        }
        else
        {
            foreach (var ext in FontFile.Extensions)
            {
                var formatted = fontFile.FileNameWithExtension(ext);
                if (_fontRegistrar.GetFont(formatted) is string filePath)
                    return filePath;
            }
        }

        return fontFile.PostScriptName;
    }
#nullable disable
    //iOS Bug Fix END
}
