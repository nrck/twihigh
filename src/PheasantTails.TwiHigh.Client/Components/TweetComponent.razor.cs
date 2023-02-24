using Microsoft.AspNetCore.Components;
using PheasantTails.TwiHigh.DataStore.Entity;

namespace PheasantTails.TwiHigh.Client.Components
{
    public partial class TweetComponent
    {
        [Parameter]
        public Tweet? Tweet { get; set; }

        private string CreateAt
        {
            get
            {
                if (Tweet == null)
                {
                    return string.Empty;
                }

                // 1年前ならyyyy/mm/dd
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddYears(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("yyyy/MM/dd");
                }

                // 24時間より前ならm/d
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddDays(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("M/d");
                }

                // 1時間より前なら H:mm
                if (Tweet.CreateAt <= DateTimeOffset.UtcNow.AddHours(-1))
                {
                    return Tweet.CreateAt.ToLocalTime().ToString("H:mm");
                }

                // H:mm:ss
                return Tweet.CreateAt.ToLocalTime().ToString("H:mm:ss");
            }
        }
    }
}
