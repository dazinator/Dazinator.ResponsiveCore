using System;
using System.Linq.Expressions;
using Xunit;

namespace Tests
{
    public class ExpressionFuncBoolExtensionsTests
    {

        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void OrElse(bool first, bool second, bool expectedResult)
        {
            // True or else False =-
            Expression<Func<bool>> sut;
            if (first)
            {
                sut = ExpressionFuncBoolExtensions.True();
            }
            else
            {
                sut = ExpressionFuncBoolExtensions.False();
            }

            sut = sut.OrElse(() => second);
            var func = sut.Compile();
            Assert.Equal(expectedResult, func());
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void AndAlso(bool first, bool second, bool expectedResult)
        {
            // True or else False =-
            Expression<Func<bool>> sut;
            if (first)
            {
                sut = ExpressionFuncBoolExtensions.True();
            }
            else
            {
                sut = ExpressionFuncBoolExtensions.False();
            }

            sut = sut.AndAlso(() => second);
            var func = sut.Compile();
            Assert.Equal(expectedResult, func());
        }




    }
}