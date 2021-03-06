﻿using System;
using Newtonsoft.Json;
using Tweetinvi.Core.Interfaces.Credentials;

namespace Tweetinvi.Core.Interfaces.Models
{
    public class TokenRateLimit : ITokenRateLimit
    {
        private long _reset;

        [JsonProperty("remaining")]
        public int Remaining { get; private set; }

        [JsonProperty("reset")]
        public long Reset
        {
            get { return _reset; }
            set
            {
                _reset = value;
                ResetDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                ResetDateTime = ResetDateTime.AddSeconds(_reset).ToLocalTime();
                ResetDateTimeInSeconds = (ResetDateTime - DateTime.Now).TotalSeconds;
            }
        }

        [JsonProperty("limit")]
        public int Limit { get; private set; }

        [JsonIgnore]
        public double ResetDateTimeInSeconds { get; private set; }

        [JsonIgnore]
        public DateTime ResetDateTime { get; private set; }

        public override string ToString()
        {
            return string.Format("Limit:{0} | Remaining:{1} | ResetDateTime:{2} ", Limit, Remaining, ResetDateTime);
        }
    }
}