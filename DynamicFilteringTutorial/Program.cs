using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DynamicFilteringTutorial
{
    public class EntityMetadataService<T>
    {
        Dictionary<string, Expression<Func<T, object>>> _whiteList;
        public EntityMetadataService(Dictionary<string, Expression<Func<T, object>>> whiteList)
        {
            this._whiteList = whiteList;
        }

        public Expression<Func<T, object>> GetMemberExpression(string memberName)
        {
            Expression<Func<T, object>> memberExpression;
            if (false == this._whiteList.TryGetValue(memberName, out memberExpression))
            {
                throw new ArgumentException($"Error no filterable property called {memberName}");
            }

            return memberExpression;
        }
    }

    public class TestData
    {
        public int Length { get; set; }
        public string Name { get; set; }
        public string Ethnicity { get; set; }
    }

    public enum FilterCondition
    {
        Equal,
    }

    public enum ExpressionCombine
    {
        And,
        Or
    }

    public interface IFilterResolver
    {
        Expression<Func<T, bool>> ResolveFilter<T>(EntityMetadataService<T> metadataService);
    }

    public class FilterParameterDTO : IFilterResolver
    {
        public string PropertyName { get; set; }
        public FilterCondition FilterCondition { get; set; }
        public string Value { get; set; }

        public Expression<Func<T, bool>> ResolveFilter<T>(EntityMetadataService<T> metadataService)
        {
            var memberExpression = metadataService.GetMemberExpression(this.PropertyName);
            return memberExpression.BuildBinaryExpression<T>(this.FilterCondition, this.Value);
        }
    }

    public class BinaryExpressionDTO : IFilterResolver
    {
        public IFilterResolver Left { get; set; }
        public IFilterResolver Right { get; set; }
        public ExpressionCombine Combinor { get; set; }

        public Expression<Func<T, bool>> ResolveFilter<T>(EntityMetadataService<T> metadataService)
        {
            var leftExpression = this.Left.ResolveFilter(metadataService);
            var rightExpression = this.Right.ResolveFilter(metadataService);
            return leftExpression.Combine(rightExpression, this.Combinor);
        }
    }



    class Program
    {
        //Note: we do not allow filtering on ethinicity here to test our white list
        static Dictionary<string, Expression<Func<TestData, object>>> testDataWhiteList = new Dictionary<string, Expression<Func<TestData, object>>>
        {
            [nameof(TestData.Length)] = x => x.Length,
            [nameof(TestData.Name)] = x => x.Name,
        };

        static void Main(string[] args)
        {

            var test = new TestData { Length = 3, Name = "Win", Ethnicity = "Human" };
            var test1 = new TestData { Length = 4, Name = "Lose", Ethnicity = "Human" };
            var test2 = new TestData { Length = 5, Name = "Test", Ethnicity = "Human" };
            var test3 = new TestData { Length = 6, Name = "Foo", Ethnicity = "Human" };

            var filterableData = new List<TestData> { test, test1, test2, test3 };

            var simpleFilter = new FilterParameterDTO
            {
                PropertyName = nameof(TestData.Name),
                FilterCondition = FilterCondition.Equal,
                Value = "Win"
            };

            var entityMetadataService = new EntityMetadataService<TestData>(testDataWhiteList);
            var expressionFilter = simpleFilter.ResolveFilter(entityMetadataService);

            var results = filterableData.AsQueryable().Where(expressionFilter).ToList();

            Console.WriteLine($"Number of Results {results.Count}");
            Console.WriteLine($"Is Expected Result: { results.First().Equals(test)}");

            var simpleFilter2 = new FilterParameterDTO
            {
                PropertyName = nameof(TestData.Length),
                FilterCondition = FilterCondition.Equal,
                Value = "6"
            };

            var binaryExpression = new BinaryExpressionDTO { Left = simpleFilter, Right = simpleFilter2, Combinor = ExpressionCombine.Or };

            var binaryExpressionFilter = binaryExpression.ResolveFilter(entityMetadataService);

            results = filterableData.AsQueryable().Where(binaryExpressionFilter).ToList();
            Console.WriteLine($"Number of Results {results.Count}");
            Console.WriteLine($"Is Expected First Result { results.First().Equals(test)}");
            Console.WriteLine($"Is Expected Second Result { results.Skip(1).First().Equals(test3)}");

            Console.ReadLine();
        }

    }
}
