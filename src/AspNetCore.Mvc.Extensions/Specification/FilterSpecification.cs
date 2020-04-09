using AspNetCore.Mvc.Extensions.Mapping;
using AutoMapper;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace AspNetCore.Mvc.Extensions.Specification
{
    //Domain

    //Allows encapsulation of domain knowledge in a single place and re-use/extend it in data retrieval (Query), validation (Command), construction (Provider examples by brute force of all possible alternatives). Only use when have at least 2 out of 3 uses cases OR for data retrival when you want to keep Core domain knowledge in specification and allow additional user filters using WhereSpecification.
    //Specification = Single domain model, DRY principle
    //CQRS =  Seperate domain model, Loose coupling

    //Specification<Movie> spec = Specification<Movie>.All;
    //if(ForKidsOnly) spec = spec.And(new MoveForKidsSpecificaion());
    //if(OnCD) spec = spec.And(new AvailableOnCDSpecification());
    //spec.ToExpresion()

    //var spec = new MoveForKidsSpecificaion();
    //if(!spec.IsSatisfiedBy(movie))

    //Expression = Code = Can be translated. e.g expression > sql
    //Func = Compiled Code = Can't be translated

    public abstract class FilterSpecification
    {
        public bool IsValid { get; protected set; } = true;

        public abstract LambdaExpression ToLambdaExpression();

        public abstract bool IsSatisfiedBy(object entity);
    }

    public abstract class FilterSpecification<T> : FilterSpecification
    {
        //Application Service: PageNo, PageSize
        //Repository: Skip, Take
        //Use in repository public IReadOnlyList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Use in repository public CountList<T> GetList(IncludesSpecification<T> includesSpecification, FilterSpecification<T> filterSpecification, OrderBySpecification<T> orderBySpecification, int? skip = null, int? take = null)
        //Forces Encapsulation

        public static readonly FilterSpecification<T> All = new AllFilterSpecification<T>();

        //Use in repository public IReadOnlyList<T> GetList(Specification<T> specification)
        //Forces Encapsulation

        public override LambdaExpression ToLambdaExpression() => ToExpression();

        public abstract Expression<Func<T, bool>> ToExpression();

        //Use in Validation

        public override bool IsSatisfiedBy(object entity)
        {
            if(entity is T castEntity)
            {
                return IsSatisfiedBy(castEntity);
            }
            else
            {
                throw new Exception("Invalid object type");
            }
        }

        public bool IsSatisfiedBy(T entity)
        {
            Func<T, bool> predictate = ToExpression().Compile();
            return predictate(entity);
        }

        public FilterSpecification<T> Where(Expression<Func<T, bool>> expression)
        {
            var specification = new WhereFilterSpecification<T>(expression);

            if (this == All)
                return specification;

            return new AndFilterSpecification<T>(this, specification);
        }

        public FilterSpecification<T> And(FilterSpecification<T> specification)
        {
            if (this == All)
                return specification;

            if (specification == All)
                return this;

            return new AndFilterSpecification<T>(this, specification);
        }

        public FilterSpecification<T> Or(FilterSpecification<T> specification)
        {
            if (this == All || specification == All)
                return All;

            return new OrFilterSpecification<T>(this, specification);
        }

        public FilterSpecification<T> Not(FilterSpecification<T> specification)
        {
            return new NotFilterSpecification<T>(this);
        }

        public FilterSpecification<TDestination> Map<TDestination>(IMapper mapper)
        {
            return new WhereFilterSpecification<TDestination>(AutoMapperHelper.MapWhereClause<T, TDestination>(mapper, ToExpression()));
        }
    }

    //var spec =  new MoveForKidsSpecificaion().Where(t => t.Name = name).ToExpression(); 

    internal sealed class WhereFilterSpecification<T> : FilterSpecification<T>
    {
        private readonly Expression<Func<T, bool>> _expression;

        public WhereFilterSpecification(Expression<Func<T, bool>> expression)
        {
            _expression = expression;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            return _expression;
        }
    }

    internal sealed class AllFilterSpecification<T> : FilterSpecification<T>
    {
        public override Expression<Func<T, bool>> ToExpression()
        {
            return x => true;
        }
    }

    internal sealed class AndFilterSpecification<T> : FilterSpecification<T>
    {
        private readonly FilterSpecification<T> _left;
        private readonly FilterSpecification<T> _right;

        public AndFilterSpecification(FilterSpecification<T> left, FilterSpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> leftExpression = _left.ToExpression();
            Expression<Func<T, bool>> rightExpression = _right.ToExpression();

            BinaryExpression andExpression = Expression.AndAlso(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters.Single());
        }
    }

    internal sealed class OrFilterSpecification<T> : FilterSpecification<T>
    {
        private readonly FilterSpecification<T> _left;
        private readonly FilterSpecification<T> _right;

        public OrFilterSpecification(FilterSpecification<T> left, FilterSpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> leftExpression = _left.ToExpression();
            Expression<Func<T, bool>> rightExpression = _right.ToExpression();

            BinaryExpression andExpression = Expression.OrElse(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters.Single());
        }
    }

    internal sealed class NotFilterSpecification<T> : FilterSpecification<T>
    {
        private readonly FilterSpecification<T> _specification;

        public NotFilterSpecification(FilterSpecification<T> specification)
        {
            _specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> expression = _specification.ToExpression();

            UnaryExpression notExpression = Expression.Not(expression.Body);

            return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters.Single());
        }
    }
}
