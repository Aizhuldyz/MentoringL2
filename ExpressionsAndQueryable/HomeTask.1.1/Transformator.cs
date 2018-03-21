using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HomeTask._1._1
{
    public class TraceExpressionVisitor : ExpressionVisitor
    {
        public int indent;

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return base.Visit(node);

            Console.WriteLine("{0}{1} - {2}", new string(' ', indent * 4),
                node.NodeType, node.GetType());

            indent++;
            var result = base.Visit(node);
            indent--;

            return result;
        }
    }

    public class BinaryIncrementVisitor : ExpressionVisitor
    {
        public Dictionary<string, object> ReplacementParams;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Add || node.NodeType == ExpressionType.Subtract)
            {
                ParameterExpression param = null;
                ConstantExpression constant = null;
                if (node.Left.NodeType == ExpressionType.Parameter && node.Right.NodeType == ExpressionType.Constant)
                {
                    param = (ParameterExpression) node.Left;
                    constant = node.Right as ConstantExpression;
                }

                if (param != null && constant != null && constant.Type == typeof(int) && (int) constant.Value == 1)
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.Add:
                            return Expression.Increment(param);
                        case ExpressionType.Subtract:
                            return Expression.Decrement(param);
                    }
                }
            }

            return base.VisitBinary(node);
        }
    }

    public class LambdaParamVisitor : ExpressionVisitor
    {
        public Dictionary<string, object> ReplacementParams;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (ReplacementParams.Select(x => x.Key).Contains(node.Name))
            {
                return Expression.Constant(ReplacementParams.FirstOrDefault(_ => _.Key == node.Name).Value);
            }
            return base.VisitParameter(node); 
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var updatedParams = node.Parameters.GroupJoin(ReplacementParams,
                    p => p.Name, r => r.Key, (p, r) => new
                    {
                        param = p.Name,
                        index = node.Parameters.IndexOf(p),
                        replacement = r.FirstOrDefault()
                    })
                .Where(_ => _.replacement.Key == null)
                .Select(p => Expression.Parameter(node.Parameters[p.index].Type, p.param)).ToArray();
           
            return Expression.Lambda(Visit(node.Body), updatedParams);
               
        }

        public LambdaExpression Modify(Expression expression, Dictionary<string, object> dictionary)
        {
            ReplacementParams = dictionary;
            return (LambdaExpression)Visit(expression);
        }


    }
}
