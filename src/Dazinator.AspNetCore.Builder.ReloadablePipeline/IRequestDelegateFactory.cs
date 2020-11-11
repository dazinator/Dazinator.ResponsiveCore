using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public interface IRequestDelegateFactory: IDisposable
    {
        void Initialise(RequestDelegate onNext);
        Task<RequestDelegate> GetRequestDelegateTask();
    }
}