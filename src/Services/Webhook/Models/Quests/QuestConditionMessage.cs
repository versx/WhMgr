namespace WhMgr.Services.Webhook.Models.Quests
{
    using System.Text.Json.Serialization;

    using QuestConditionType = POGOProtos.Rpc.QuestConditionProto.Types.ConditionType;

    public sealed class QuestConditionMessage
    {
        [JsonPropertyName("type")]
        public QuestConditionType Type { get; set; }

        [JsonPropertyName("info")]
        public QuestCondition Info { get; set; }

        public QuestConditionMessage()
        {
            Type = QuestConditionType.Unset;
        }
    }
}