using System;
using Microsoft.AspNetCore.Http;

namespace Dazinator.AspNetCore.Builder.ReloadablePipeline
{
    public interface IRequestDelegateFactory: IDisposable
    {
        void Initialise(RequestDelegate onNext);
        RequestDelegate Get();
    }
}