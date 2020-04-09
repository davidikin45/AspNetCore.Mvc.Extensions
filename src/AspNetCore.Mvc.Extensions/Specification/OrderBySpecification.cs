using AspNetCore.Mvc.Extensions.Mapping;
using AspNetCore.Mvc.Extensions.OrderByMapping;
using AutoMapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.Specification
{
    public abstract class OrderBySpecification
    {
        public bool IsValid { get; protected set; } = true;

        public abstract LambdaExpression ToLambdaExpression();

        public string OrderByString { get; protected set; }
    }

    public abstract class OrderBySpecification<T> : OrderBySpecification
    {
        //Application Service: PageNo, PageSize
        //Repository: Skip, Take
        //Use in repository public IReadOnlyList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Use in repository public CountList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Forces Encapsulation

        public static readonly OrderBySpecification<T> Nothing = new NothingOrderBySpecification<T>();

        public override LambdaExpression ToLambdaExpression() => ToExpression() as LambdaExpression;

        public abstract Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> ToExpression();

        public OrderBySpecification<T> ThenBy(Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> expression)
        {
            var specification = new SingleOrderBySpecification<T>(expression);

            if (this == Nothing)
                return specification;

            return new ThenByOrderBySpecification<T>(this, specification);
        }

        public OrderBySpecification<T> ThenBy(OrderBySpecification<T> specification)
        {
            if (this == Nothing)
                return specification;

            if (specification == Nothing)
                return this;

            return new ThenByOrderBySpecification<T>(this, specification);
        }

        public OrderBySpecification<TDestination> Map<TDestination>(IMapper mapper)
        {
            return new SingleOrderBySpecification<TDestination>(AutoMapperHelper.MapOrderBy<T, TDestination>(mapper, ToExpression()));
        }

        public OrderBySpecification<TDestination> Map<TDestination>(IOrderByMapper mapper)
        {
            return new SingleOrderBySpecification<TDestination>(mapper.GetOrderBy<T, TDestination>(OrderByString));
        }
    }

    internal sealed class SingleOrderBySpecification<T> : OrderBySpecification<T>
    {
        private readonly Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> _expression;

        public SingleOrderBySpecification(Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> expression)
        {
            _expression = expression;
        }

        public override Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> ToExpression() => _expression;
    }

    internal sealed class DummyOrderedQueryable<T> : IOrderedQueryable<T>
    {

        private readonly IQueryable<T> _queryable;
        public DummyOrderedQueryable(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        public Type ElementType => _queryable.ElementType;

        public Expression Expression => _queryable.Expression;

        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _queryable.GetEnumerator();
    }

    internal sealed class NothingOrderBySpecification<T> : OrderBySpecification<T>
    {
        public override Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> ToExpression()
        {
            return (q => new DummyOrderedQueryable<T>(q));
        }
    }

    internal sealed class ThenByOrderBySpecification<T> : OrderBySpecification<T>
    {
        private readonly OrderBySpecification<T> _left;
        private readonly OrderBySpecification<T> _right;

        public ThenByOrderBySpecification(OrderBySpecification<T> left, OrderBySpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> ToExpression()
        {
            Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> leftExpression = _left.ToExpression();
            Expression<Func<IQueryable<T>, IOrderedQueryable<T>>> rightExpression = _right.ToExpression();

            return Combine(leftExpression, rightExpression);
        }
        private static Expression<Func<TSource, TDestination>> Combine<TSource, TDestination>(
            params Expression<Func<TSource, TDestination>>[] selectors)
        {
            var param = Expression.Parameter(typeof(TSource), "x");
            return Expression.Lambda<Func<TSource, TDestination>>(
                Expression.MemberInit(
                    Expression.New(typeof(TDestination).GetConstructor(Type.EmptyTypes)),
                    from selector in selectors
                    let replace = new ParameterReplaceVisitor(
                          selector.Parameters[0], param)
                    from binding in ((MemberInitExpression)selector.Body).Bindings
                          .OfType<MemberAssignment>()
                    select Expression.Bind(binding.Member,
                          replace.VisitAndConvert(binding.Expression, "Combine")))
                , param);
        }
        private class ParameterReplaceVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression from, to;
            public ParameterReplaceVisitor(ParameterExpression from, ParameterExpression to)
            {
                this.from = from;
                this.to = to;
            }
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return node == from ? to : base.VisitParameter(node);
            }
        }

    }
}
