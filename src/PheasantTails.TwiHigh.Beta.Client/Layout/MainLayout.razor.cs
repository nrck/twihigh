using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Beta.Client.Services;

namespace PheasantTails.TwiHigh.Beta.Client.Layout
{
    public partial class MainLayout : LayoutComponentBase
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        protected IMessageService MessageService { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。

        protected override void OnInitialized()
        {
            base.OnInitialized();
            MessageService.OnChangedMessage = StateHasChanged;
        }
    }
}
