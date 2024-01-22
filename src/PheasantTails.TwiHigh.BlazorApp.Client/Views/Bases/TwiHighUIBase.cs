﻿using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Reactive.Bindings;

namespace PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;

public class TwiHighUIBase : ComponentBase
{
    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    public async Task InvokeRenderAsync() => await InvokeAsync(StateHasChanged);

    public IDisposable SubscribeStateHasChanged<T>(ReactiveCommand<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());

    public IDisposable SubscribeStateHasChanged<T>(AsyncReactiveCommand<T> command) => command.Subscribe(async (_) => await InvokeRenderAsync());
}
