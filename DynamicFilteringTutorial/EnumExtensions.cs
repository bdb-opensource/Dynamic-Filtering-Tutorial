using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DynamicFilteringTutorial
{
    public static class EnumExtensions
    {
        public static Func<Expression, Expression, BinaryExpression> GetBinaryExpressionBuilder(this FilterCondition filterCondition)
        {
            switch (filterCondition)
            {
                case FilterCondition.Equal:
                    return Expression.Equal;
                default:
                    throw new ArgumentException($"Filter Condition {filterCondition} is not resolvable");
            }
        }

        public static Func<Expression, Expression, BinaryExpression> GetCombineExpressionDelegate(this ExpressionCombine combinor)
        {
            switch (combinor)
            {
                case ExpressionCombine.And:
                    return Expression.And;
                case ExpressionCombine.Or:
                    return Expression.Or;
                default:
                    throw new NotImplementedException();
            }
        }

    }

}
