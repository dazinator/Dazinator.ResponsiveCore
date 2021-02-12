using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public interface IRebuildStrategy
    {
        void Invalidate();
        Task<RequestDelegate> Get();
        void Initialise(Func<RequestDelegate> buildDelegate);
    }

}