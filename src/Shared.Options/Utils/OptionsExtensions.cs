using System;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Primitives
{
    public static class CompositeChangeTokenFactoryBuilderExtensions
    {

        public static CompositeChangeTokenFactoryBuilder IncludeOptions<TOptions>(
        this CompositeChangeTokenFactoryBuilder builder,
        IOptionsMonitor<TOptions> optionsMontitor,
        Func<TOptions, string, bool> shouldTriggerChangeToken = null)
        {
            builder.IncludeSubscribingHandlerTrigger(handler => optionsMontitor.OnChange((o, a) =>
            {
                if (shouldTriggerChangeToken(o, a))
                {
                    handler?.Invoke();
                }
            }));
            return builder;
        }
        public static CompositeChangeTokenFactoryBuilder IncludeOptions<TOptions>(
           this CompositeChangeTokenFactoryBuilder builder,
           IOptionsMonitor<TOptions> optionsMontitor)
        {
            return IncludeOptions(builder, optionsMontitor);
        }

        public static CompositeChangeTokenFactoryBuilder IncludeOptions<TOptions>(
            this CompositeChangeTokenFactoryBuilder builder,
            IOptionsMonitor<TOptions> optionsMontitor,
            string optionsNameFilter)
        {
            return IncludeOptions(builder, optionsMontitor, (o, a) => a == optionsNameFilter);
        }

        public static CompositeChangeTokenFactoryBuilder IncludeOptions<TOptions>(
           this CompositeChangeTokenFactoryBuilder builder,
           IOptionsMonitor<TOptions> optionsMontitor,
           Predicate<TOptions> shouldTriggerChangeToken)
        {
            return IncludeOptions(builder, optionsMontitor, (o, a) => shouldTriggerChangeToken(o));
        }   
    }
}
