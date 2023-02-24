using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Client.TypedHttpClients;

namespace PheasantTails.TwiHigh.Client.Pages
{
    public partial class Login
    {
#pragma warning disable CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。
        [Inject]
        private AppUserHttpClient AppUserHttpClient { get; set; }
#pragma warning restore CS8618 // null 非許容のフィールドには、コンストラクターの終了時に null 以外の値が入っていなければなりません。Null 許容として宣言することをご検討ください。


    }
}
