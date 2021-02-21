namespace System.Linq.Expressions
{
    public static class ExpressionFuncBoolExtensions
    {
        public static Expression<Func<bool>> True() { return () => true; }
        public static Expression<Func<bool>> False() { return () => false; }

        public static Expression<Func<bool>> OrElse(this Expression<Func<bool>> expr1,
                                                            Expression<Func<bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        public static Expression<Func<bool>> AndAlso(this Expression<Func<bool>> expr1,
                                                             Expression<Func<bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }

}