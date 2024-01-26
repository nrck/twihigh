namespace PheasantTails.TwiHigh.BlazorApp.Client.Services;

public interface IScrollInfoService
{
    /// <summary>
    /// Fire actions on scroll event.
    /// </summary>
    event Action<string[]>? OnScroll;

    /// <summary>
    /// Disable scroll event handling.
    /// </summary>
    ValueTask Disable();

    /// <summary>
    /// Enable scroll event handling.
    /// </summary>
    ValueTask Enable();
}