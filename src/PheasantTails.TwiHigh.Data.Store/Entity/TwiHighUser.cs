﻿namespace PheasantTails.TwiHigh.Data.Store.Entity
{
    public class TwiHighUser : BaseEntity
    {
        public const string PARTITION_KEY = "/id";

        public string DisplayId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string Biography { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long Tweets { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;
        public Guid[] Follows { get; set; } = Array.Empty<Guid>();
        public Guid[] Followers { get; set; } = Array.Empty<Guid>();
    }
}
