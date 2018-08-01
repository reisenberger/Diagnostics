// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Diagnostics.HealthChecks
{
    public class HealthCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly HealthCheckOptions _healthCheckOptions;
        private readonly IHealthCheckService _healthCheckService;
        private readonly IHealthCheck[] _checks;

        public HealthCheckMiddleware(
            RequestDelegate next,
            IOptions<HealthCheckOptions> healthCheckOptions,
            IHealthCheckService healthCheckService)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            if (healthCheckOptions == null)
            {
                throw new ArgumentNullException(nameof(healthCheckOptions));
            }

            if (healthCheckService == null)
            {
                throw new ArgumentNullException(nameof(healthCheckService));
            }

            _next = next;
            _healthCheckOptions = healthCheckOptions.Value;
            _healthCheckService = healthCheckService;

            _checks = FilterHealthChecks(_healthCheckService.Checks, healthCheckOptions.Value.HealthCheckNames);
        }

        /// <summary>
        /// Processes a request.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }

            // Get results
            var result = await _healthCheckService.CheckHealthAsync(_checks, httpContext.RequestAborted);

            // Map status to response code - this is customizable via options. 
            if (!_healthCheckOptions.ResultStatusCodes.TryGetValue(result.Status, out int statusCode))
            {
                var message =
                    $"No status code mapping found for {nameof(HealthCheckStatus)} value: {result.Status}." +
                    $"{nameof(HealthCheckOptions)}.{nameof(HealthCheckOptions.ResultStatusCodes)} must contain" +
                    $"and entry for {result.Status}.";

                throw new InvalidOperationException(message);
            }

            httpContext.Response.StatusCode = statusCode;

            if (_healthCheckOptions.ResponseWriter != null)
            {
                await _healthCheckOptions.ResponseWriter(httpContext, result);
            }
        }

        private static IHealthCheck[] FilterHealthChecks(
            IReadOnlyDictionary<string, IHealthCheck> checks,
            ISet<string> names)
        {
            // If there are no filters then include all checks.
            if (names.Count == 0)
            {
                return checks.Values.ToArray();
            }

            // Keep track of what we don't find so we can report errors.
            var notFound = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
            var matches = new List<IHealthCheck>();

            foreach (var kvp in checks)
            {
                if (!notFound.Remove(kvp.Key))
                {
                    // This check was excluded
                    continue;
                }

                matches.Add(kvp.Value);
            }

            if (notFound.Count >0)
            {
                var message = 
                    $"The following health checks were not found: '{string.Join(", ", notFound)}'." +
                    $"Registered health checks: '{string.Join(", ", checks.Keys)}'.";
                throw new InvalidOperationException(message);
            }

            return matches.ToArray();
        }
    }
}
