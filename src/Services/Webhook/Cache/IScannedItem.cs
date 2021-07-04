namespace WhMgr.Services.Webhook.Cache
{
    internal interface IScannedItem
    {
        double Latitude { get; }

        double Longitude { get; }
    }
}