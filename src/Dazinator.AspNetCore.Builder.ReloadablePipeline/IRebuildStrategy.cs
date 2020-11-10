using System;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public interface IRebuildStrategy
    {
        void Invalidate();
        RequestDelegate Get();
        void Initialise(Func<RequestDelegate> buildDelegate);
    }    

}