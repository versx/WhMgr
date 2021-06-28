namespace WhMgr.Services.Subscriptions.Models
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("metadata")]
    public class Metadata
    {
        [
            Column("key"),
            Key
        ]
        public string Key { get; set; }

        [
            Column("value"),
            DefaultValue(null)
        ]
        public string Value { get; set; }
    }
}