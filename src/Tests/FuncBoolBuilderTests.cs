using System.Linq.Expressions;
using Xunit;

namespace Tests
{
    public class FuncBoolBuilderTests
    {


        [Theory]
        [InlineData(true, false, true)]
        [InlineData(false, true, true)]
        [InlineData(false, false, false)]
        public void FuncBoolBuilder_OrElse(bool first, bool second, bool expectedResult)
        {
            // True or else False =-
            var result = FuncBoolBuilder.Build((builder) =>
            {
                builder.Initial(() => first)
                       .OrElse(() => second);
            });

            Assert.Equal(expectedResult, result());
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void FuncBoolBuilder_AndAlso(bool first, bool second, bool expectedResult)
        {
            // True or else False =-
            var result = FuncBoolBuilder
                            .Build((builder) =>
                            {
                                builder.Initial(first)
                                       .AndAlso(() => second);
                            });

            Assert.Equal(expectedResult, result());
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void FuncBoolBuilder_AndAlso_SubExpression(bool first, bool subSecond, bool expectedResult)
        {
            // True or else False =-
            var result = FuncBoolBuilder
                        .Build((builder) =>
                         {
                             builder.Initial(first)
                                    .AndAlso(sub =>
                                    {
                                        sub.Initial(() => subSecond);
                                    });
                         });

                            
            Assert.Equal(expectedResult, result());
        }



    }
}