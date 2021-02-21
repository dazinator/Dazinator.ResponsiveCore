using System;

namespace Microsoft.Extensions.Primitives
{
    //public static class ChangeTokenFactoryUtils
    //{
    //    public static Func<IChangeToken> CreateChangeTokenFactoryFromConfiguration<TOptions>(this IServiceProvider serviceProvider)
    //    {
    //        var config = serviceProvider.GetRequiredService<IConfiguration>();

    //        Func<IChangeToken> result = () =>
    //        {
    //            return config.GetReloadToken();
    //        };

    //        return result;

    //    }

    //    public static Func<IChangeToken> CreateChangeTokenFactory<TOptions>(this IConfiguration config)
    //    {
    //        Func<IChangeToken> result = () =>
    //        {
    //            return config.GetReloadToken();
    //        };

    //        return result;
    //    }
    //}
}