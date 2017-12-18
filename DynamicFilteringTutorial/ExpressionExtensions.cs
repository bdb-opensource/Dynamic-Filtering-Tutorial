using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DynamicFilteringTutorial
{
    public static class ExpressionExtensions
    {
        public static LambdaExpression StripConvert(this LambdaExpression expression)
        {
            if (ExpressionType.Convert != expression.Body.NodeType)
            {
                return expression;
            }
            return Expression.Lambda(((UnaryExpression)expression.Body).Operand, expression.Parameters); ;
        }

        public static PropertyInfo ToPropertyInfo(this MemberExpression memberExpression)
        {
            return (PropertyInfo)memberExpression.Member;
        }

        public static Expression<Func<T, bool>> BuildBinaryExpression<T>(this Expression<Func<T, object>> expression, FilterCondition filterCondition, string value)
        {
            MemberExpression memberExpression = (expression.StripConvert().Body as MemberExpression);

            var propertyInfo = memberExpression.ToPropertyInfo();
            var resultType = propertyInfo.PropertyType;
            var constValue = resultType.Parse(value);
            var constant = Expression.Constant(constValue, resultType);

            var parameter = Expression.Parameter(typeof(T));

            var property = Expression.Property(parameter, propertyInfo);
            var binaryExpressionBuilder = filterCondition.GetBinaryExpressionBuilder();
            var binaryExpression = binaryExpressionBuilder(property, constant);

            return Expression.Lambda<Func<T, bool>>(binaryExpression, parameter);
        }

        public static Expression<Func<T1, T2>> ReplaceParameter<T1, T2>(this Expression<Func<T1, T2>> expression, ParameterExpression param)
        {
            var replacedExpression = new ReplaceVisitor(expression.Parameters.First(), param).Visit(expression.Body);
            return Expression.Lambda<Func<T1, T2>>(replacedExpression, param);
        }

        public static Expression<Func<T1, T2>> Combine<T1, T2>(this Expression<Func<T1, T2>> first, Expression<Func<T1, T2>> second, ExpressionCombine combinor)
        {
            var param = Expression.Parameter(typeof(T1), "x");
            var newFirst = first.ReplaceParameter(param);
            var newSecond = second.ReplaceParameter(param);

            var expressionCombinor = combinor.GetCombineExpressionDelegate();
            var combinedExpression = expressionCombinor(newFirst.Body, newSecond.Body);

            return Expression.Lambda<Func<T1, T2>>(combinedExpression, param);
        }

    }
}
