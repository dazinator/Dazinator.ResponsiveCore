using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class ResponsiveHttpClientFactoryTests
    {

        [Fact]
        public async Task Can_Get_Named_HttpClient()
        {
            // arrange
            var sut = TestHelper.CreateTestSubject<IHttpClientFactory>(out var testServices, (services) =>
            {
                services.AddHttpClient("foo", (h) => h.BaseAddress = new Uri("http://foo.locahost"));
            });

            // act
            using var httpClient = sut.CreateClient("foo");
            Assert.NotNull(httpClient);           
        }

        [Fact]
        public async Task Can_Add_NamedHttpClient_AfterDIContainerBuilt()
        {
            // arrange

            var settings = new Dictionary<string, string>();
            var httpClientName = "v1-BaseAddress";
            var initialBaseAddress = "http://foo.localhost";
            settings.Add(httpClientName, initialBaseAddress);

            // HttpClientFactoryOptions
            // can maybe take advantage of: HttpClientFactoryOptions
            // plus https://github.com/dazinator/Dazinator.Extensions.Options.Globbing
            // the idea is that
            // 1. When requesting a http client - a version suffix will be present in the name e.g "foo-v1"
            //    and when the configuration is changed and needs to apply, this will increment i.e "foo-v2".
            // 2. At runtime, consumer code will track the name that is "current". This will be used by the consumer to get the http client.
            //    for example, if a user updates the settings, the new current name might be "foo-v3" and the application will now
            //    ask for "foo-v3" named client
            // 3. The `DefaultHttpClientFactory` provided by microsoft, as seen here: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Http/src/DefaultHttpClientFactory.cs#L131
            //    already resolves a named options (of type: HttpClientFactoryOptions) with the requested name, when configuring a new http client.
            //    for example it will ask for named options with name "foo-v2"
            // 4. You would normally have to register these named options ahead of time on startup.
            //    However we could take advantage of the ability to provide newly named `HttpClientFactoryOptions` at runtime
            //    for example, a custom OptionsFactory like this one: https://github.com/dazinator/Dazinator.Extensions.Options.Globbing/blob/master/src/Options.Globbing/GlobMatchingNamedOptionsFactory.cs#L23
            //    could be implemented, but rather than being able to access only names configured in advance (injected) it could also hook into
            //    a service to provide the "dynamic" set of names which could be added to at runtime (e.g foo-v1, foo-v2 etc).
                        
           
            var sut = TestHelper.CreateTestSubject<IHttpClientFactory>(out var testServices, (services) =>
            {
              //  var name = $"foo-{currentHttpClientVersion}";
              //  var setting = settings[httpClientName];
              //  services.AddHttpClient(httpClientName, (h) => h.BaseAddress = initialBaseAddress);
            });

            // dynamicall add httpclient.
            settings.Add("foo", initialBaseAddress);
            using var httpClient = sut.CreateClient("foo");
            Assert.NotNull(httpClient);           
            Assert.Equal(initialBaseAddress, httpClient.BaseAddress.ToString());         

        }

    }

   
}