using Microsoft.Extensions.DependencyInjection;

namespace StoreGuard.Common.Services
{
    public static class DepInj
    {
        public static void ConfigureServiceOptions<TOptions>(
            this IServiceCollection services,
            Action<IServiceProvider, TOptions> configure)
            where TOptions : class
        {
            services
                .AddOptions<TOptions>()
                .Configure<IServiceProvider>((options, resolver) => configure(resolver, options));
        }
    }
}