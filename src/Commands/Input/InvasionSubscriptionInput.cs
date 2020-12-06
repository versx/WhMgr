namespace WhMgr.Commands.Input
{
    using DSharpPlus.CommandsNext;

    internal class InvasionSubscriptionInput : SubscriptionInput
    {
        private readonly CommandContext _context;

        public InvasionSubscriptionInput(CommandContext ctx) : base(ctx)
        {
            _context = ctx;
        }
    }
}