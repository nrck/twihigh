namespace PheasantTails.TwiHigh.Beta.Client.Services
{
    public interface IScrollInfoService
    {
        /// <summary>
        /// スクロール発生時のイベントハンドラ
        /// </summary>
        event EventHandler<string[]>? OnScroll;

        /// <summary>
        /// 無効にします
        /// </summary>
        /// <returns></returns>
        ValueTask Disable();

        /// <summary>
        /// 有効にします
        /// </summary>
        /// <returns></returns>
        ValueTask Enable();
    }
}