namespace PheasantTails.TwiHigh.BlazorApp.Client.Exceptions;

public class TwiHighException : Exception
{
    /// <summary>
    /// 使用ユーザに向けた画面上に表示するメッセージ
    /// </summary>
    public string DisplayMessage
    {
        get
        {
            if (string.IsNullOrEmpty(displayMessage))
            {
                return Message;
            }
            return displayMessage;
        }
        set => displayMessage = value;
    }
    private string displayMessage = string.Empty;

    public TwiHighException()
    {
    }

    public TwiHighException(string? message) : base(message)
    {
    }

    public TwiHighException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
