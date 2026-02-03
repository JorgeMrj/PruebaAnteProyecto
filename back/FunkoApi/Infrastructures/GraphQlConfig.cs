using FunkoApi.Graphql.Mutations;
using FunkoApi.Graphql.Queries;
using FunkoApi.Graphql.Subscriptions;
using FunkoApi.Graphql.Types;
using FunkoApi.Graphql.Publishers;
using HotChocolate.Execution.Configuration;
using Serilog;

namespace FunkoApi.Infrastructures;

/// <summary>
/// Extensiones de configuración de GraphQL con HotChocolate.
/// </summary>
public static class GraphQlConfig
{
    /// <summary>
    /// Configura GraphQL con queries de productos y categorías.
    /// </summary>
    public static IRequestExecutorBuilder AddGraphQl(this IServiceCollection services, IWebHostEnvironment environment)
    {
        Log.Information("🔍 Configurando GraphQL con HotChocolate...");
        services.AddGraphQlPubSub();
        return services
            .AddGraphQLServer()
            .AddAuthorization() 
            .AddProjections()
            .AddQueryType<FunkoQuery>()
            .AddMutationType<FunkoMutation>()
            .AddSubscriptionType<FunkoSubscription>()
            .AddInMemorySubscriptions()
            .AddType<FunkoType>()
            .AddType<CategoryType>()
            .AddMutationConventions() 
            .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = environment.IsDevelopment());
    }
}