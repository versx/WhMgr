namespace WhMgr.Services.Webhook.Models.Quests
{
    using System.Text.Json.Serialization;

    using QuestRewardType = POGOProtos.Rpc.QuestRewardProto.Types.Type;

    public sealed class QuestRewardMessage
    {
        [JsonPropertyName("type")]
        public QuestRewardType Type { get; set; }

        [JsonPropertyName("info")]
        public QuestReward Info { get; set; }

        public QuestRewardMessage()
        {
            Type = QuestRewardType.Unset;
        }
    }
}