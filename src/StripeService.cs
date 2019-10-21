namespace WhMgr
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Stripe;

    using WhMgr.Diagnostics;

    public class StripeService
    {
        #region Variables

        private static readonly IEventLogger _logger = EventLogger.GetLogger();

        private readonly RequestOptions _requestOptions;
        private readonly CustomerService _customerService;
        private string _lastCustomerId;
        private static List<Customer> _customers;

        #endregion

        #region Constructor

        public StripeService(string apiKey)
        {
            _customerService = new CustomerService();
            _requestOptions = new RequestOptions { ApiKey = apiKey };
            StripeConfiguration.ApiKey = apiKey;
        }

        #endregion

        #region Public Methods

        public Customer GetCustomer(ulong guildId, ulong userId)
        {
            var customers = GetAllCustomers();//_customerService.List(_customerListOptions, _requestOptions);
            var customerObj = customers./*Data.*/FirstOrDefault(x =>
                              ulong.TryParse(x.Metadata["user_server_discord_id"], out var discordGuildId) && discordGuildId == guildId &&
                              ulong.TryParse(x.Metadata["user_discord_id"], out var discordUserId) && discordUserId == userId);
            return customerObj;
        }

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

        public CustomerData GetCustomerData(ulong guildId, ulong userId)
        {
            /*
            "metadata": {
              "role_discord_id": "617924024449761291",
              "role_name": "Donor2",
              "server_id": "574451996656926720",
              "server_name": "OC Scans"
            },
            */
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

        private DateTime? GetExpireDate(Customer customer)
        {
            if (customer?.Subscriptions?.Data?.Count > 0)
            {
                var latestSubscription = customer.Subscriptions.Data[customer.Subscriptions.Data.Count - 1];
                return latestSubscription.CurrentPeriodEnd;
            }
            return null;
        }

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

    public class CustomerData
    {
        public ulong UserId { get; set; }

        public ulong GuildId { get; set; }

        public DateTime? ExpireDate { get; set; }

        public string ServerName { get; set; }

        public string RoleName { get; set; }
    }
}