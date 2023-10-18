// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

global using System.Collections.ObjectModel;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Text.RegularExpressions;
global using System.Web;
global using System.Xml;
global using System.Xml.Linq;
global using CommunityToolkit.Maui;
global using CommunityToolkit.Maui.Alerts;
global using CommunityToolkit.Maui.Core.Primitives;
global using CommunityToolkit.Maui.Views;
global using CommunityToolkit.Mvvm.ComponentModel;
global using CommunityToolkit.Mvvm.Input;
global using CommunityToolkit.Mvvm.Messaging;
global using CommunityToolkit.Mvvm.Messaging.Messages;
global using M3U8Parser;
global using MetroLog.Maui;
global using MetroLog.Operators;
global using NerdNewsNavigator2.Data;
global using NerdNewsNavigator2.Messages;
global using NerdNewsNavigator2.Model;
global using NerdNewsNavigator2.Services;
global using NerdNewsNavigator2.View;
global using NerdNewsNavigator2.Interface;
global using NerdNewsNavigator2.ViewModel;
global using SQLite;
global using YoutubeExplode;
global using NerdNewsNavigator2.Devices;
global using NerdNewsNavigator2.Extensions;
global using MetroLog;
global using LoggerFactory = MetroLog.LoggerFactory;
global using NerdNewsNavigator2.Primitives;
global using System.Windows.Input;
#if ANDROID || IOS
global using Plugin.LocalNotification;
#endif

#if ANDROID
global using Plugin.LocalNotification.AndroidOption;
global using NerdNewsNavigator2.Platforms.Android;
#endif
