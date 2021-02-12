using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Dazinator.ResponsiveCore.ReloadablePipeline
{
    public abstract class RequestDelegateUtils
    {
        public static RequestDelegate BuildRequestDelegate(IApplicationBuilder builder, RequestDelegate onNext, Action<IApplicationBuilder> configure, bool isTerminal)
        {
            var subBuilder = builder.New();

            configure(subBuilder);

            // if nothing in this pipeline runs, join back to root pipeline?
            if (!isTerminal && onNext != null)
            {
                subBuilder.Run(onNext);
                //_subBuilder.Run(async (http) => await onNext());
            }
            var newInstance = subBuilder.Build();
            return newInstance;
        }
    }


}