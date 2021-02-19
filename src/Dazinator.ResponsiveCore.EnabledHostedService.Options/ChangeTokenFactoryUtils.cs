using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Primitives
{
    public static class ChangeTokenFactoryUtils
    {
        public static Func<IChangeToken> CreateChangeTokenFactoryFromOptionsMonitor<TOptions>(this IServiceProvider serviceProvider)
        {
            var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TOptions>>();
            var changeTokenFactory = ChangeTokenFactoryHelper.CreateChangeTokenFactory((onChangedCallback) =>
            {
                return monitor.OnChange(a => onChangedCallback());
            });
            return changeTokenFactory;
        }

        public static Func<IChangeToken> CreateChangeTokenFactory<TOptions>(this IOptionsMonitor<TOptions> monitor)
        {
            var changeTokenFactory = ChangeTokenFactoryHelper.CreateChangeTokenFactory((onChangedCallback) =>
            {
                return monitor.OnChange(a => onChangedCallback());
            });
            return changeTokenFactory;
        }

        public static Func<IChangeToken> CreateChangeTokenFactoryFromConfiguration<TOptions>(this IServiceProvider serviceProvider)
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            Func<IChangeToken> result = () =>
            {
                return config.GetReloadToken();
            };

            return result;

        }

        public static Func<IChangeToken> CreateChangeTokenFactory<TOptions>(this IConfiguration config)
        {
            Func<IChangeToken> result = () =>
            {
                return config.GetReloadToken();
            };

            return result;
        }
    }
}