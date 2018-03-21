using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace HomeTask2
{
	public class ExpressionToFtsRequestTranslator : ExpressionVisitor
	{
		private StringBuilder _resultString;
	    private IList<string> _queries;

	    public ExpressionToFtsRequestTranslator(IList<string> queries)
	    {
	        _queries = queries;	        
        }

	    public IList<string> TranslateQuery(Expression exp)
	    {
	        Translate(exp);
	        return _queries;
	    }

	    public void Translate(Expression exp)
		{
		    _resultString = new StringBuilder();
            Visit(exp);
		    if (_resultString.Length != 0)
		    {
		        _queries.Add(_resultString.ToString());
		    }
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.DeclaringType == typeof(Queryable)
				&& node.Method.Name == "Where")
			{
				var predicate = node.Arguments[1];
				Visit(predicate);

				return node;
			}

            if (node.Method.DeclaringType == typeof(string)) {
                var predicate = node.Arguments[0];
                string newConstantValue = "";
                switch (node.Method.Name)
                {
                    case "StartsWith":
                            newConstantValue = ((ConstantExpression)predicate).Value + "*";
                            break;
                    case "EndsWith":
                            newConstantValue = "*" + ((ConstantExpression)predicate).Value;
                            break;
                    case "Contains":
                            newConstantValue = "*" + ((ConstantExpression)predicate).Value + "*";
                            break;
                }
                if (newConstantValue != "") {
                    BuildQueryString(node.Object, Expression.Constant(newConstantValue));
                    return node; 
                }

            }

            return base.VisitMethodCall(node);
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			switch (node.NodeType)
			{
				case ExpressionType.Equal:
                    if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)                        
                    {
                        BuildQueryString(node.Left, node.Right);
                        break;
                    }
                    if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess) {
                        BuildQueryString(node.Right, node.Left);
                        break;
                    }
                    throw new NotSupportedException($"Operation {node.NodeType} is not supported");
                case ExpressionType.AndAlso:
                    new ExpressionToFtsRequestTranslator(_queries).Translate(node.Left);
                    new ExpressionToFtsRequestTranslator(_queries).Translate(node.Right);
                    break;
                default:
					throw new NotSupportedException($"Operation {node.NodeType} is not supported");
			};

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			_resultString.Append(node.Member.Name).Append(":");

			return base.VisitMember(node);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			_resultString.Append(node.Value);

			return node;
		}

        private void BuildQueryString(Expression member, Expression constant)
        {
            Visit(member);
            _resultString.Append("(");
            Visit(constant);
            _resultString.Append(")");
        }


    }
}
