using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HomeTask._1._1
{
    class Program
    {
        static void Main(string[] args)
        {
            Expression<Func<int, int>> expression = (a) => a + 1;
            var transformed = new BinaryIncrementVisitor().VisitAndConvert(expression, "");
            new TraceExpressionVisitor().Visit(transformed);
            Console.WriteLine("Invoke Example: " + transformed.Compile().Invoke(3));

            Expression<Func<int, int>> expressionsub = (a) => a - 1;
            var transformedsub = new BinaryIncrementVisitor().VisitAndConvert(expressionsub, "");
            new TraceExpressionVisitor().Visit(transformedsub);
            Console.WriteLine("Invoke Example: " + transformedsub.Compile().Invoke(3));
            
            Expression<Func<int, int, int, int>> lambdaExpression = (a, b, c) => a * (b - c);
            var dictionary = new Dictionary<string, object> { { "a", 2 }, { "b", 5 }, {"c", 9} };

            var lambdaExpressionTransformed = new LambdaParamVisitor().Modify(lambdaExpression, dictionary);
            new TraceExpressionVisitor().Visit(lambdaExpressionTransformed);

            Console.WriteLine("Invoke Example: " + (Expression<Func<int, int>>)lambdaExpressionTransformed.Compile().DynamicInvoke());
            Console.ReadLine();
        }
    }
}
