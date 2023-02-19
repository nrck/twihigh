using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.Model;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class PostFormComponent
    {
        [Parameter]
        public string UserAvatarUrl { get; set; } = string.Empty;

        public PostTweetContext PostTweetContext { get; set; }

        protected override Task OnInitializedAsync()
        {
            PostTweetContext = new PostTweetContext();
            return base.OnInitializedAsync();
        }

        private void OnSubmit()
        {
            Console.WriteLine("OnSubmit:" + PostTweetContext.Text);
        }
    }
}
