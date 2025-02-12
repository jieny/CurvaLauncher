﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CurvaLauncher.Messages;
using CurvaLauncher.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CurvaLauncher.Models;

public partial class QueryResultModel : ObservableObject
{
    private readonly IQueryResult _rawQueryResult;

    private QueryResultModel(float weight, string title, string description, ImageSource? icon, IQueryResult rawQueryResult)
    {
        Weight = weight;
        Title = title;
        Description = description;
        _icon = icon;
        _rawQueryResult = rawQueryResult;
    }

    private ImageSource? _icon;

    public float Weight { get; }
    public string Title { get; }
    public string Description { get; }
    public ImageSource? Icon => _icon;

    public void SetFallbackIcon(Func<ImageSource> iconFactory)
    {
        if (_icon == null)
        {
            SetProperty(ref _icon, iconFactory.Invoke(), nameof(Icon));
        }
        else if (_icon is BitmapImage bitmapImage && bitmapImage.IsDownloading)
        {
            var originIcon = _icon;

            SetProperty(ref _icon, iconFactory.Invoke(), nameof(Icon));
            bitmapImage.DownloadCompleted += (s, e) =>
            {
                SetProperty(ref _icon, originIcon, nameof(Icon));
            };
        }
    }

    [RelayCommand]
    public async Task Invoke()
    {
        App.ServiceProvider
            .GetRequiredService<IMessenger>()
            .Send(SaveQueryMessage.Instance);

        if (_rawQueryResult is ISyncQueryResult syncQueryResult)
        {
            try
            {
                syncQueryResult.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "CurvaLauncher Result Invoke failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else if (_rawQueryResult is IAsyncQueryResult asyncQueryResult)
        {
            try
            {
                await asyncQueryResult.InvokeAsync(App.GetLauncherCancellationToken());
            }
            catch (OperationCanceledException)
            {
                // pass
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "CurvaLauncher Result Invoke failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        App.CloseLauncher();
    }

    public static QueryResultModel Create(CurvaLauncherPluginInstance pluginInstance, IQueryResult queryResult)
    {
        return new QueryResultModel(pluginInstance.Weight * queryResult.Weight, queryResult.Title, queryResult.Description, queryResult.Icon, queryResult);
    }
}
