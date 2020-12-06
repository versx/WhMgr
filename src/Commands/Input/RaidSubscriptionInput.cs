namespace WhMgr.Commands.Input
{
    using DSharpPlus.CommandsNext;

    internal class RaidSubscriptionInput : SubscriptionInput
    {
        private readonly CommandContext _context;

        public RaidSubscriptionInput(CommandContext ctx) : base(ctx)
        {
            _context = ctx;
        }
    }
}