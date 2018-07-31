// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods for registering <see cref="IHealthCheck"/> instances in an <see cref="IHealthChecksBuilder"/>.
    /// </summary>
    public static class HealthChecksBuilderAddCheckExtensions
    {
        /// <summary>
        /// Adds a new health check with the specified name and implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="name">The name of the health check, which should indicate the component being checked.</param>
        /// <param name="check">A delegate which provides the code to execute when the health check is run.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, string name, Func<CancellationToken, Task<HealthCheckResult>> check)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (check == null)
            {
                throw new ArgumentNullException(nameof(check));
            }
            
            return builder.AddCheck(new HealthCheck(name, check));
        }

        /// <summary>
        /// Adds a new health check with the specified name and implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="name">The name of the health check, which should indicate the component being checked.</param>
        /// <param name="check">A delegate which provides the code to execute when the health check is run.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, string name, Func<Task<HealthCheckResult>> check)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (check == null)
            {
                throw new ArgumentNullException(nameof(check));
            }

            return builder.AddCheck(name, _ => check());
        }

        /// <summary>
        /// Adds a new health check with the implementation.
        /// </summary>
        /// <param name="builder">The <see cref="IHealthChecksBuilder"/> to add the check to.</param>
        /// <param name="check">An <see cref="IHealthCheck"/> implementation.</param>
        /// <returns>The <see cref="IHealthChecksBuilder"/>.</returns>
        public static IHealthChecksBuilder AddCheck(this IHealthChecksBuilder builder, IHealthCheck check)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (check == null)
            {
                throw new ArgumentNullException(nameof(check));
            }

            builder.Services.AddSingleton<IHealthCheck>(check);
            return builder;
        }
    }
}
