using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Moq;
using backend_user.Extensions;
using backend_user.Services.Abstractions;
using backend_user.Services;
using System.Security.Claims;

namespace backend_user.tests.Extensions
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAuthenticationServices_ShouldRegisterAllRequiredServices()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add required dependencies for OAuth2AuthenticationStrategy
            services.AddScoped<IOAuth2Service>(provider => Mock.Of<IOAuth2Service>());
            services.AddLogging();

            // Act
            services.AddAuthenticationServices();

            // Assert
            var serviceProvider = services.BuildServiceProvider();

            // Verify all core services are registered
            serviceProvider.GetService<ISecurityHeadersService>().Should().NotBeNull();
            serviceProvider.GetService<IAuthorizationPathService>().Should().NotBeNull();
            serviceProvider.GetService<IClaimsBuilderService>().Should().NotBeNull();
            serviceProvider.GetService<IAuthenticationContextService>().Should().NotBeNull();

            // Verify strategy is registered
            var strategies = serviceProvider.GetServices<IAuthenticationStrategy>();
            strategies.Should().NotBeNull();
            strategies.Should().HaveCount(1);
            strategies.First().Should().BeOfType<OAuth2AuthenticationStrategy>();
        }

        [Fact]
        public void AddAuthenticationServices_ShouldRegisterServicesWithCorrectLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAuthenticationServices();

            // Assert
            var securityHeadersDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISecurityHeadersService));
            securityHeadersDescriptor.Should().NotBeNull();
            securityHeadersDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var authorizationPathDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthorizationPathService));
            authorizationPathDescriptor.Should().NotBeNull();
            authorizationPathDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var claimsBuilderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IClaimsBuilderService));
            claimsBuilderDescriptor.Should().NotBeNull();
            claimsBuilderDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var authContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthenticationContextService));
            authContextDescriptor.Should().NotBeNull();
            authContextDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);

            var strategyDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthenticationStrategy));
            strategyDescriptor.Should().NotBeNull();
            strategyDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void AddAuthenticationServices_ShouldRegisterServicesWithCorrectImplementations()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddAuthenticationServices();

            // Assert
            var securityHeadersDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISecurityHeadersService));
            securityHeadersDescriptor!.ImplementationType.Should().Be(typeof(SecurityHeadersService));

            var authorizationPathDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthorizationPathService));
            authorizationPathDescriptor!.ImplementationType.Should().Be(typeof(AuthorizationPathService));

            var claimsBuilderDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IClaimsBuilderService));
            claimsBuilderDescriptor!.ImplementationType.Should().Be(typeof(ClaimsBuilderService));

            var authContextDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthenticationContextService));
            authContextDescriptor!.ImplementationType.Should().Be(typeof(AuthenticationContextService));

            var strategyDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthenticationStrategy));
            strategyDescriptor!.ImplementationType.Should().Be(typeof(OAuth2AuthenticationStrategy));
        }

        [Fact]
        public void AddAuthenticationServices_ShouldReturnServiceCollection_ForMethodChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddAuthenticationServices();

            // Assert
            result.Should().BeSameAs(services);
        }

        [Fact]
        public void AddCustomAuthenticationStrategy_ShouldRegisterCustomStrategy()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCustomAuthenticationStrategy<TestAuthenticationStrategy>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var strategies = serviceProvider.GetServices<IAuthenticationStrategy>();
            
            strategies.Should().NotBeNull();
            strategies.Should().HaveCount(1);
            strategies.First().Should().BeOfType<TestAuthenticationStrategy>();
        }

        [Fact]
        public void AddCustomAuthenticationStrategy_ShouldRegisterWithScopedLifetime()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCustomAuthenticationStrategy<TestAuthenticationStrategy>();

            // Assert
            var strategyDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IAuthenticationStrategy));
            strategyDescriptor.Should().NotBeNull();
            strategyDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
            strategyDescriptor.ImplementationType.Should().Be(typeof(TestAuthenticationStrategy));
        }

        [Fact]
        public void AddCustomAuthenticationStrategy_ShouldReturnServiceCollection_ForMethodChaining()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            var result = services.AddCustomAuthenticationStrategy<TestAuthenticationStrategy>();

            // Assert
            result.Should().BeSameAs(services);
        }

        [Fact]
        public void AddAuthenticationServices_AndCustomStrategy_ShouldRegisterBothStrategies()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add required dependencies
            services.AddScoped<IOAuth2Service>(provider => Mock.Of<IOAuth2Service>());
            services.AddLogging();

            // Act
            services.AddAuthenticationServices()
                   .AddCustomAuthenticationStrategy<TestAuthenticationStrategy>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var strategies = serviceProvider.GetServices<IAuthenticationStrategy>().ToList();
            
            strategies.Should().HaveCount(2);
            strategies.Should().Contain(s => s.GetType() == typeof(OAuth2AuthenticationStrategy));
            strategies.Should().Contain(s => s.GetType() == typeof(TestAuthenticationStrategy));
        }

        [Fact]
        public void AddCustomAuthenticationStrategy_ShouldAllowMultipleCustomStrategies()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddCustomAuthenticationStrategy<TestAuthenticationStrategy>()
                   .AddCustomAuthenticationStrategy<AnotherTestAuthenticationStrategy>();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var strategies = serviceProvider.GetServices<IAuthenticationStrategy>().ToList();
            
            strategies.Should().HaveCount(2);
            strategies.Should().Contain(s => s.GetType() == typeof(TestAuthenticationStrategy));
            strategies.Should().Contain(s => s.GetType() == typeof(AnotherTestAuthenticationStrategy));
        }

        [Fact]
        public void AddAuthenticationServices_ShouldAllowAuthenticationContextService_ToResolveStrategies()
        {
            // Arrange
            var services = new ServiceCollection();
            
            // Add required dependencies
            services.AddScoped<IOAuth2Service>(provider => Mock.Of<IOAuth2Service>());
            services.AddLogging();

            // Act
            services.AddAuthenticationServices();

            // Assert
            var serviceProvider = services.BuildServiceProvider();
            var authContextService = serviceProvider.GetService<IAuthenticationContextService>();
            
            authContextService.Should().NotBeNull();
            authContextService.Should().BeOfType<AuthenticationContextService>();
        }
    }

    // Test authentication strategies for testing purposes
    public class TestAuthenticationStrategy : IAuthenticationStrategy
    {
        public bool CanHandle(HttpContext context) => false;
        public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context) => Task.FromResult<ClaimsPrincipal?>(null);
    }

    public class AnotherTestAuthenticationStrategy : IAuthenticationStrategy
    {
        public bool CanHandle(HttpContext context) => false;
        public Task<ClaimsPrincipal?> AuthenticateAsync(HttpContext context) => Task.FromResult<ClaimsPrincipal?>(null);
    }
}
