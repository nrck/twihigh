using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using PheasantTails.TwiHigh.BlazorApp.Client.ViewModels;
using PheasantTails.TwiHigh.BlazorApp.Client.Views.Bases;
using System.ComponentModel.DataAnnotations;

namespace PheasantTails.TwiHigh.BlazorApp.Views.Pages;

public partial class Signup : TwiHighPageBase
{
    /// <summary>
    /// Login form model for EditContext.
    /// </summary>
    private class SignupFormModel
    {
        /// <summary>
        /// User`s email.
        /// </summary>
        [Required(ErrorMessage = "メールアドレスを入力してください。")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// User`s display id.<br />
        /// ex) `nr_ck`
        /// </summary>
        [Required(ErrorMessage = "アカウントIDを入力してください。")]
        public string DisplayId { get; set; } = string.Empty;

        /// <summary>
        /// User`s login password.
        /// </summary>
        [Required(ErrorMessage = "パスワードを入力してください。")]
        public string PlainPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// On post, this variable set from the login form.
    /// </summary>
    [SupplyParameterFromForm]
    private SignupFormModel Model { get; set; } = default!;

    /// <summary>
    /// Gets or sets the SignupViewModel.
    /// </summary>
    [Inject]
    public ISignupViewModel ViewModel { get; set; } = default!;

    /// <summary>
    /// The EditContext for login form.
    /// </summary>
    private EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Initializes the component.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        // If get method, set default value because Model is null.
        Model ??= new SignupFormModel();
        EditContext = new EditContext(Model);
    }

    /// <summary>
    /// On submit handler method.
    /// </summary>
    private async Task HandleValidSubmitAsync(EditContext editContext)
    {
        ViewModel.Email.Value = Model.Email;
        ViewModel.DisplayId.Value = Model.DisplayId;
        ViewModel.Password.Value = Model.PlainPassword;
        await ViewModel.SignupCommand.ExecuteAsync(editContext).ConfigureAwait(false);
    }
}
