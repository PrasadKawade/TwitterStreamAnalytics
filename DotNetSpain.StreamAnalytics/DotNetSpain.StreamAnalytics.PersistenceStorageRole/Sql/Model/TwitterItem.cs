namespace DotNetSpain.StreamAnalytics.PersistenceStorageRole.Sql.Model
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public class TwitterItem
    {

        public int Id { get; set; }

        [DataMember(Name = "id")]
        public string TwittId { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { get; set; }

        [DataMember(Name = "userid")]
        public string UserId { get; set; }

        [DataMember(Name = "profile_image_url")]
        public string ProfileImageUrl { get; set; }

        [DataMember(Name = "name")]
        public string UserName { get; set; }

        public string Path { get; set; }
    }
}
