using System;
using Microsoft.Extensions.DependencyInjection;

namespace Tests
{
    public static class TestHelper
    {
        public static TTestSubject CreateTestSubject<TTestSubject>(out IServiceProvider testServices, Action<IServiceCollection> configureTestServices = null)
        {
            IServiceCollection services = new ServiceCollection();
            configureTestServices?.Invoke(services);
            var sp = services.BuildServiceProvider();
            testServices = sp;
            var factory = sp.GetRequiredService<TTestSubject>();
            return factory;
        }

    }
}