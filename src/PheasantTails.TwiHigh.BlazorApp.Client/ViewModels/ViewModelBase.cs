using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using System.Reactive.Disposables;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

/// <summary>
/// ViewModel Base class
/// </summary>
public abstract class ViewModelBase : IViewModelBase
{
    /// <summary>
    /// This property set objects that to be disposed of all at once using the <see cref="Dispose"/>.
    /// </summary>
    protected readonly CompositeDisposable _disposable = [];

    /// <summary>
    /// Provides an abstraction for querying and managing URI navigation.
    /// </summary>
    protected readonly NavigationManager _navigationManager;

    /// <summary>
    /// Display messages(info, warn, error...) to the user in the browser.
    /// </summary>
    protected readonly IMessageService _messageService;

    public ViewModelBase(NavigationManager navigationManager, IMessageService messageService)
    {
        _navigationManager = navigationManager;
        _messageService = messageService;

        // First, initialize the Command or properties in ViewModel.
        Initialize();
        // Next, subscribe to the Command in ViewModel.
        Subscribe();
    }

    /// <summary>
    /// Dispose of all objects that need to be disposed of at once.
    /// </summary>
    public virtual void Dispose()
    {
        _disposable.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Handle exceptions in ViewModel.
    /// </summary>
    protected void HandleException(Exception ex)
        => _messageService.SetErrorMessage($"申し訳ありません。予期せぬエラーが発生しました。役立つメッセージ「{ex.GetType().Name}: {ex.Message}」");

    /// <summary>
    /// Initialize the Command or properties in ViewModel.
    /// </summary>
    protected abstract void Initialize();

    /// <summary>
    /// Subscribe to the Command in ViewModel.
    /// </summary>
    protected abstract void Subscribe();
}
