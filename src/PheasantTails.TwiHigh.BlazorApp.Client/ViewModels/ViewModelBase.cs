using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.BlazorApp.Client.Services;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;

public abstract class ViewModelBase : IViewModelBase
{
    protected readonly CompositeDisposable _disposable = [];
    protected readonly NavigationManager _navigationManager;
    protected readonly IMessageService _messageService;

    public event PropertyChangedEventHandler? PropertyChanged;

    public ViewModelBase(NavigationManager navigationManager, IMessageService messageService)
    {
        _navigationManager = navigationManager;
        _messageService = messageService;
        Initialize();
        Subscribe();
    }

    public virtual void Dispose()
    {
        _disposable.Dispose();
        GC.SuppressFinalize(this);
    }

    protected void HandleException(Exception ex)
        => _messageService.SetErrorMessage($"申し訳ありません。予期せぬエラーが発生しました。役立つメッセージ「{ex.GetType().Name}: {ex.Message}」");

    protected abstract void Initialize();

    protected abstract void Subscribe();
}
