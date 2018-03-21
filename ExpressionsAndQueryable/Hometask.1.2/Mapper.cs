using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;


namespace Hometask._1._2
{
    public class Mapper<TSource, TDestination>
    {
        private readonly Func<TSource, TDestination> _mapFunction;
        internal Mapper(Func<TSource, TDestination> func) { _mapFunction = func; }
        public TDestination Map(TSource source) { return _mapFunction(source); }
    }

    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var destinationType = typeof(TDestination);
            var sourceAsParameterExpression = Expression.Parameter(typeof(TSource));
            var initExpresssion = Expression.MemberInit(Expression.New(destinationType), 
                MapProperties(sourceAsParameterExpression, destinationType));
            var mapFunction =
                Expression.Lambda<Func<TSource, TDestination>>(
                    initExpresssion,
                    sourceAsParameterExpression
                );

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }

        public List<MemberAssignment> MapProperties(ParameterExpression sourceParameterExpression, Type destinationType)
        {
            var sourceProperties = sourceParameterExpression.Type.GetProperties().Where(p => p.CanRead).ToList();
            var destinationProperties = destinationType.GetProperties().Where(p => p.CanWrite).ToList();
            var memberBindings = sourceProperties.Join(destinationProperties,
                    s => s.Name, d => d.Name, (s, d) =>
                        new
                        {
                            destinationParameter = d,
                            sourceMemberExpression = Expression.MakeMemberAccess(sourceParameterExpression, s)
                        })
                .Select(_ => Expression.Bind(_.destinationParameter, _.sourceMemberExpression))
                .ToList();
            return memberBindings;
        }
    }

}
