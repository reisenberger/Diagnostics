using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly.CircuitBreaker;

namespace Microsoft.Extensions.Diagnostics.HealthChecks.Polly
{
    /// <summary>
    /// Provides extension methods for registering <see cref="IHealthCheck"/> instances for Polly <see cref="ICircuitBreakerPolicy"/> policies.
    /// </summary>
    public static class HealthChecksBuilderAddCircuitBreakerCheckExtensions
    {
        /// <summary>
        /// Adds a new health check for the supplied Polly <see cref="ICircuitBreakerPolicy"/> policy, using the <see cref="M:policy.PolicyKey"/> as the name of the health check.
        /// <remarks>Circuits in <see cref="CircuitState.Closed"/> return a <see cref="HealthCheckStatus.Healthy"/> status.  All other <see cref="CircuitState"/> return <see cref="HealthCheckStatus.Degraded"/>.</remarks>
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="policy">The <see cref="ICircuitBreakerPolicy"/> whose status should be checked.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCircuitBreakerCheck(this IHealthChecksBuilder builder, ICircuitBreakerPolicy policy)
        {
            return builder.AddCircuitBreakerCheck(policy.PolicyKey, policy);
        }

        /// <summary>
        /// Adds a new health check for the supplied Polly <see cref="ICircuitBreakerPolicy"/> policy, with the specified name.
        /// <remarks>Circuits in <see cref="CircuitState.Closed"/> return a <see cref="HealthCheckStatus.Healthy"/> status.  All other <see cref="CircuitState"/> return <see cref="HealthCheckStatus.Degraded"/>.</remarks>
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="policy">The <see cref="ICircuitBreakerPolicy"/> whose status should be checked.</param>
        /// <param name="name">The name of the health check, which should indicate the component being checked.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCircuitBreakerCheck(this IHealthChecksBuilder builder, string name, ICircuitBreakerPolicy policy)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return builder.AddCheck(name, () => GetCircuitHealth(policy));
        }

        private static Task<HealthCheckResult> GetCircuitHealth(ICircuitBreakerPolicy policy)
        {
            return GetCircuitHealth(policy.CircuitState);
        }

        private static Task<HealthCheckResult> GetCircuitHealth(CircuitState circuitState)
        {
            switch (circuitState)
            {
                case CircuitState.Closed:
                    return Task.FromResult(HealthCheckResult.Healthy());
                case CircuitState.HalfOpen:
                case CircuitState.Open:
                case CircuitState.Isolated:
                    return Task.FromResult(HealthCheckResult.Degraded($"CircuitState.{circuitState}"));
                default:
                    throw new Exception($"Unknown CircuitState: CircuitState.{circuitState}");
            }
        }
    }
}
