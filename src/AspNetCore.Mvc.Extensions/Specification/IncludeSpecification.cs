using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.Specification
{
    public abstract class IncludeSpecification
    {
        public bool IsValid { get; protected set; } = true;

        public abstract LambdaExpression ToLambdaExpression();
    }

    public abstract class IncludeSpecification<T> : IncludeSpecification
    {
        //Application Service: PageNo, PageSize
        //Repository: Skip, Take
        //Use in repository public IReadOnlyList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Use in repository public CountList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Forces Encapsulation

        public static readonly IncludeSpecification<T> Nothing = new NothingIncludesSpecification<T>();

        public override LambdaExpression ToLambdaExpression() => ToExpression() as LambdaExpression;

        public abstract Expression<Func<T, object>>[] ToExpression();

        public IncludeSpecification<T> Include(Expression<Func<T, object>> expression)
        {
            var specification = new SingleIncludeSpecification<T>(expression);

            if (this == Nothing)
                return specification;

            return new AndIncludesSpecification<T>(this, specification);
        }

        public IncludeSpecification<T> And(IncludeSpecification<T> specification)
        {
            if (this == Nothing)
                return specification;

            if (specification == Nothing)
                return this;

            return new AndIncludesSpecification<T>(this, specification);
        }

        public IncludeSpecification<TDestination> Map<TDestination>(IMapper mapper)
        {
            return new MultipleIncludeSpecification<TDestination>(AutoMapperHelper.MapIncludes<T, TDestination>(mapper, ToExpression()));
        }
    }

    internal sealed class MultipleIncludeSpecification<T> : IncludeSpecification<T>
    {
        private readonly Expression<Func<T, object>>[] _expression;

        public MultipleIncludeSpecification(Expression<Func<T, object>>[] expression)
        {
            _expression = expression;
        }

        public override Expression<Func<T, object>>[] ToExpression()
        {
            return _expression;
        }
    }

    internal sealed class SingleIncludeSpecification<T> : IncludeSpecification<T>
    {
        private readonly Expression<Func<T, object>> _expression;

        public SingleIncludeSpecification(Expression<Func<T, object>> expression)
        {
            _expression = expression;
        }

        public override Expression<Func<T, object>>[] ToExpression()
        {
            return new Expression<Func<T, object>>[] { _expression };
        }
    }

    internal sealed class NothingIncludesSpecification<T> : IncludeSpecification<T>
    {
        public override Expression<Func<T, object>>[] ToExpression()
        {
            return new Expression<Func<T, Object>>[] { };
        }
    }

    internal sealed class AndIncludesSpecification<T> : IncludeSpecification<T>
    {
        private readonly IncludeSpecification<T> _left;
        private readonly IncludeSpecification<T> _right;

        public AndIncludesSpecification(IncludeSpecification<T> left, IncludeSpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, object>>[] ToExpression()
        {
            Expression<Func<T, object>>[] leftExpression = _left.ToExpression();
            Expression<Func<T, object>>[] rightExpression = _right.ToExpression();

            return leftExpression.Concat(rightExpression).ToArray();
        }
    }
}
