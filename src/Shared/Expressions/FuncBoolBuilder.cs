
namespace System.Linq.Expressions
{
    public class FuncBoolBuilder : IFuncBoolBuilderInitial, IFuncBoolBuilder
    {
        private Expression<Func<bool>> _expression = null;
        private Lazy<FuncBoolBuilder> _subBuilder = new Lazy<FuncBoolBuilder>(() => new FuncBoolBuilder());

        private FuncBoolBuilder()
        {

        }

        private FuncBoolBuilder(Expression<Func<bool>> expression)
        {
            _expression = expression;
        }

        public static IFuncBoolBuilderInitial Create()
        {
            return new FuncBoolBuilder();
        }


        public static IFuncBoolBuilder Create(Expression<Func<bool>> expression)
        {
            return new FuncBoolBuilder(expression);
        }

        public IFuncBoolBuilder Initial(Expression<Func<bool>> expression)
        {
            _expression = expression;
            return this;
        }

        public IFuncBoolBuilder AndAlso(Expression<Func<bool>> expression)
        {
            _expression = _expression.AndAlso(expression);
            return this;
        }

        public IFuncBoolBuilder AndAlso(Action<IFuncBoolBuilderInitial> buildSubExpression)
        {
            var subBuilder = _subBuilder.Value;
            buildSubExpression(subBuilder);
            var subExpression = subBuilder._expression;
            subBuilder.Reset();
            _expression = _expression.AndAlso(subExpression);
            return this;
        }

        public IFuncBoolBuilder OrElse(Expression<Func<bool>> expression)
        {
            _expression = _expression.OrElse(expression);
            return this;
        }

        public IFuncBoolBuilder OrElse(Action<IFuncBoolBuilderInitial> buildSubExpression)
        {
            var subBuilder = _subBuilder.Value;
            buildSubExpression(subBuilder);
            var subExpression = subBuilder._expression;
            subBuilder.Reset();
            _expression = _expression.OrElse(subExpression);
            return this;
        }

        public Func<bool> Build()
        {
            if (_expression == null)
            {
                throw new InvalidOperationException("expression not yet set, invalid for build.");
            }
            return _expression.Compile();
        }

        private void Reset()
        {
            _expression = null;
        }

    }


    public interface IFuncBoolBuilderInitial
    {
        IFuncBoolBuilder Initial(Expression<Func<bool>> expression);
    }

    public interface IFuncBoolBuilder
    {
        IFuncBoolBuilder AndAlso(Expression<Func<bool>> expression);
        IFuncBoolBuilder AndAlso(Action<IFuncBoolBuilderInitial> buildSubExpression);
        IFuncBoolBuilder OrElse(Expression<Func<bool>> expression);
        IFuncBoolBuilder OrElse(Action<IFuncBoolBuilderInitial> buildSubExpression);
        Func<bool> Build();
    }
}