namespace WhMgr.Data.Models
{
    using ServiceStack.DataAnnotations;

    [Alias("metadata")]
    public class Metadata
    {
        [
            Alias("key"),
            PrimaryKey
        ]
        public string Key { get; set; }

        [
            Alias("value"),
            Default(null)
        ]
        public string Value { get; set; }
    }
}