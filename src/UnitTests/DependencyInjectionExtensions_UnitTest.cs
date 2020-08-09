﻿#pragma warning disable CA1707 // Identifiers should not contain underscores
using ChatterBot.Core.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChatterBot.Tests
{
    public class DependencyInjection_Should
    {
        [WpfFact]
        [Trait("Category", "Unit")]
        public async Task BeAbleToInstantiateAllRegisteredTypes()
        {
            // Arrange
            var services = new ServiceCollection();

            var fakeAppSettings = new ApplicationSettings() { Entropy = "SomeFakedEntropyString", LightDbConnection = "Filename=database.db;Password=1234" };
            //services.AddUI();
            services.AddCore(fakeAppSettings);
            services.AddInfrastructureForLiteDb(fakeAppSettings);
            services.AddInfrastructureForTwitch();

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var result = new List<object>();

            foreach (var serviceDescriptor in services)
            {
                serviceDescriptor.ServiceType.Should().NotBeNull();
                serviceDescriptor.ServiceType.FullName.Should().NotBeNullOrEmpty();

                try
                {
                    // TODO: Fix exceptions for MediatR derived types, for now this will check everything else.
                    if (serviceDescriptor.ServiceType.FullName.StartsWith("MediatR", StringComparison.InvariantCulture))
                        continue;

                    var instance = serviceProvider.GetService(serviceDescriptor.ServiceType);
                    instance.Should().NotBeNull();
                    instance.Should().BeAssignableTo(serviceDescriptor.ServiceType);
                    result.Add(instance);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to instantiate '{serviceDescriptor.ServiceType.FullName}'", ex);
                }
            }

            // Assert
            var expectedInstanceCountExcludingMediatR = services.Count(x => x.ServiceType.FullName?.StartsWith("MediatR", StringComparison.InvariantCulture) == false);
            result.Should().HaveCount(expectedInstanceCountExcludingMediatR);
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}
#pragma warning restore CA1707 // Identifiers should not contain underscores