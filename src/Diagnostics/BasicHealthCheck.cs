namespace WhMgr.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.Extensions.Diagnostics.HealthChecks;

    /// <summary>
    /// Basic example health check class if others want to implement their own.
    /// </summary>
    public class BasicHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // Description about health check or if degraded or unhealthy, explain error(s)/issue(s)
            var description = "Test description about health check, or if degraded or unhealthy - explain errors/issues.";

            // Explanation about error
            var exception = new Exception("Explaination about error.");

            // Extra key value pair data
            var data = new Dictionary<string, object>();

            return await Task.FromResult
            (
                new HealthCheckResult
                (
                    HealthStatus.Healthy,
                    description,
                    exception,
                    data
                )
            );
        }
    }
}