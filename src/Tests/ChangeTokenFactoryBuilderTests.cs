using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Tests
{
    public class ChangeTokenFactoryBuilderTests
    {
        [Fact]
        public void EmptyBuilder_Builds_EmptyChangeTokenFactory()
        {
            var sut = new CompositeChangeTokenFactoryBuilder();
            var factory = sut.Build();

            Assert.NotNull(factory);
            var tokenA = factory();
            var tokenB = factory();
            Assert.Same(tokenB, tokenA);
            Assert.True(tokenA.ActiveChangeCallbacks);
            Assert.False(tokenA.HasChanged);
            tokenA.RegisterChangeCallback((a) =>
            {
                throw new Exception("this should never fire.");
            }, null);
        }

        [Fact]
        public async Task Include_ChangeToken()
        {
            var sut = new CompositeChangeTokenFactoryBuilder();
            TriggerChangeToken token = null;
            var factory = sut.Include(() =>
            {
                token = new TriggerChangeToken();
                return token;
            }).Build();

            var consumed = factory();
            Assert.Same(token, consumed);

            // When we trigger the token and request a new one,
            //we get a new one thats different from the previous one.
            IChangeToken newToken = null;
            IChangeToken original = token;
            token.RegisterChangeCallback(a =>
            {
                newToken = factory();
            }, null);

            token.Trigger();
            await Task.Delay(200);

            Assert.NotNull(newToken);
            Assert.NotSame(newToken, original);
        }

    }
}