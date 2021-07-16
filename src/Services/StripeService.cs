namespace WhMgr.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Stripe;

    using WhMgr.Diagnostics;

    /// <summary>
    /// Stripe service class
    /// </summary>
    public class StripeService
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger("STRIPE", Program.LogLevel);

        private readonly RequestOptions _requestOptions;
        private readonly CustomerService _customerService;
        private string _lastCustomerId;
        private static List<Customer> _customers;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="StripeService"/> class with the provided API key
        /// </summary>
        /// <param name="apiKey">Stripe API key</param>
        public StripeService(string apiKey)
        {
            _customerService = new CustomerService();
            _requestOptions = new RequestOptions { ApiKey = apiKey };
            StripeConfiguration.ApiKey = apiKey;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a customer based on guild id and user id
        /// </summary>
        /// <param name="guildId">Guild Id to filter by</param>
        /// <param name="userId">User Id to filter by</param>
        /// <returns>Customer in guild id and user id</returns>
        public Customer GetCustomer(ulong guildId, ulong userId)
        {
            var customers = GetAllCustomers();
            var customerObj = customers.FirstOrDefault(x =>
                              ulong.TryParse(x.Metadata["user_server_discord_id"], out var discordGuildId) && discordGuildId == guildId &&
                              ulong.TryParse(x.Metadata["user_discord_id"], out var discordUserId) && discordUserId == userId
            );
            return customerObj;
        }

        /// <summary>
        /// Get a list of all Stripe customers
        /// </summary>
        /// <returns>Returns a list of all Stripe customers</returns>
        public List<Customer> GetAllCustomers()
        {
            if (_customers == null || _customers?.Count == 0)
            {
                var list = new List<Customer>();
                List<Customer> result;
                while ((result = GetCustomers()).Count > 0)
                {
                    list.AddRange(result);
                }
                _customers = list;
            }

            return _customers;
        }

        /// <summary>
        /// Gets a list of Stripe customers with a maximum limit of 100
        /// </summary>
        /// <param name="limit">Maximum limit (default: 100)</param>
        /// <returns>Returns a list of Stripe customers</returns>
        public List<Customer> GetCustomers(int limit = 100)
        {
            StripeList<Customer> customers;
            if (string.IsNullOrEmpty(_lastCustomerId))
            {
                customers = _customerService.List(new CustomerListOptions { Limit = limit }, _requestOptions);
            }
            else
            {
                customers = _customerService.List(new CustomerListOptions { Limit = limit, StartingAfter = _lastCustomerId }, _requestOptions);
            }
            _lastCustomerId = customers?.LastOrDefault()?.Id;
            return customers?.Data;
        }

        /// <summary>
        /// Gets Stripe customer data by guild id and user id
        /// </summary>
        /// <param name="guildId">Guild Id to filter by</param>
        /// <param name="userId">User Id to filter by</param>
        /// <returns>Returns the customer data relating to the provided guild id and user id</returns>
        public CustomerData GetCustomerData(ulong guildId, ulong userId)
        {
            var customer = GetCustomer(guildId, userId);
            var expires = GetExpireDate(customer);
            var roleName = GetSubscriptionData(customer, "role_name");
            var serverName = GetSubscriptionData(customer, "server_name");
            return new CustomerData
            {
                GuildId = guildId,
                UserId = userId,
                ExpireDate = expires,
                RoleName = roleName,
                ServerName = serverName
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Gets the expiration date for the Stripe customer
        /// </summary>
        /// <param name="customer">Stripe customer</param>
        /// <returns>Returns subscription expiration date</returns>
        private DateTime? GetExpireDate(Customer customer)
        {
            if (customer?.Subscriptions?.Data?.Count > 0)
            {
                var latestSubscription = customer.Subscriptions.Data[customer.Subscriptions.Data.Count - 1];
                return latestSubscription.CurrentPeriodEnd;
            }
            return null;
        }

        /// <summary>
        /// Get expiration date from customer from metdata key
        /// </summary>
        /// <param name="customer">Customer class to get expiration date from</param>
        /// <param name="metadataKey">Expiration metadata key</param>
        /// <returns>Returns expiration date as string</returns>
        private string GetSubscriptionData(Customer customer, string metadataKey)
        {
            if (customer == null)
            {
                _logger.Error($"Failed to get subscription for customer.");
                return null;
            }

            var subscriptions = customer?.Subscriptions?.Data;
            if (subscriptions.Count > 0)
            {
                //customer.subscriptions.data[0].items.data[0].plan.metadata
                var latestSubscription = subscriptions[subscriptions.Count - 1];
                var items = latestSubscription.Items.Data;
                if (items.Count > 0)
                {
                    var serverName = items[items.Count - 1].Plan.Metadata[metadataKey];
                    return serverName;
                }
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// Customer data metadata
    /// </summary>
    public class CustomerData
    {
        /// <summary>
        /// Gets or sets the customer User ID
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Gets or sets the customer Guild ID
        /// </summary>
        public ulong GuildId { get; set; }

        /// <summary>
        /// Gets or sets the customer's subscription expiration date
        /// </summary>
        public DateTime? ExpireDate { get; set; }

        /// <summary>
        /// Gets or sets the Discord server name
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the purchased Discord server role name
        /// </summary>
        public string RoleName { get; set; }
    }
}