using AspNetCore.Mvc.Extensions.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.Mvc.Extensions.Data.Helpers
{
    public static class JsonQueryableExtensions
    {
        public static JsonQueryable<TEntity> AsJsonQueryable<TEntity>(this DbSet<TEntity> dbSet)
        where TEntity : class
        {
            return new JsonQueryable<TEntity>(dbSet);
        }
    }

    public class JsonQueryable<TEntity>
         where TEntity : class
    {
        private readonly IEntityType _entityType;
        private readonly DbSet<TEntity> _dbSet;
        private readonly List<(string column, string operation, string key, string param)> _Conditions = new List<(string column, string operation, string key, string param)>();

        public JsonQueryable(DbSet<TEntity> dbSet)
        {
            _dbSet = dbSet;
            var dbContext = _dbSet.GetDbContext();

            var model = dbContext.Model;
            var entityTypes = model.GetEntityTypes();
            _entityType = entityTypes.First(t => t.ClrType == typeof(TEntity));
        }

        public JsonQueryable<TEntity> Or()
        {
            _Conditions.Add(("", "Or", "", ""));

            return this;
        }

        public JsonQueryable<TEntity> And()
        {
            _Conditions.Add(("", "And", "", ""));

            return this;
        }

        public JsonQueryable<TEntity> Like(Expression<Func<TEntity, MultiLanguageString>> propertyLambda, string value)
        {
            var jsonKey = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return Like(propertyLambda, jsonKey, value);
        }

        public JsonQueryable<TEntity> Like<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            jsonKey = $".{jsonKey}";

            _Conditions.Add((col, "Like", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> StartsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
      where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            jsonKey = $".{jsonKey}";

            _Conditions.Add((col, "StartsWith", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> EndsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
      where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            jsonKey = $".{jsonKey}";

            _Conditions.Add((col, "EndsWith", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> Equals<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
        var col = _entityType.GetColumnName(propertyInfo);
        jsonKey = $".{jsonKey}";

            _Conditions.Add((col, "=", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> NotEquals<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            jsonKey = $".{jsonKey}";

            _Conditions.Add((col, "!=", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> ArrayContains<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string value)
        where TProperty : class
        {
            return ArrayContains(propertyLambda, string.Empty, value);
        }

        public JsonQueryable<TEntity> ArrayContains<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            if (!string.IsNullOrEmpty(jsonKey))
            {
                jsonKey = $".{jsonKey}";
            }

            _Conditions.Add((col, "ArrayContains", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> ArrayItemStartsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string value)
        where TProperty : class
        {
            return ArrayItemStartsWith(propertyLambda, string.Empty, value);
        }

        public JsonQueryable<TEntity> ArrayItemStartsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            if (!string.IsNullOrEmpty(jsonKey))
            {
                jsonKey = $".{jsonKey}";
            }

            _Conditions.Add((col, "ArrayItemStartsWith", jsonKey, value));

            return this;
        }

        public JsonQueryable<TEntity> ArrayItemEndsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string value)
        where TProperty : class
        {
            return ArrayItemEndsWith(propertyLambda, string.Empty, value);
        }

        public JsonQueryable<TEntity> ArrayItemEndsWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyLambda, string jsonKey, string value)
        where TProperty : class
        {
            var propertyInfo = EntityPropertyExtensions.GetPropertyInfo(propertyLambda);
            var col = _entityType.GetColumnName(propertyInfo);
            if (!string.IsNullOrEmpty(jsonKey))
            {
                jsonKey = $".{jsonKey}";
            }

            _Conditions.Add((col, "ArrayItemEndsWith", jsonKey, value));

            return this;
        }

        public List<TEntity> ToList()
        {
            return AsQueryable().ToList();
        }

        public Task<List<TEntity>> ToListAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return AsQueryable().ToListAsync(cancellationToken);
        }

        public IQueryable<TEntity> AsQueryable()
        {
            var tableName = _dbSet.GetTableName();

            var parameters = new List<object>();

            var sb = new StringBuilder();
            sb.AppendLine($"SELECT * FROM {tableName}");

            if(_Conditions.Count > 0)
            {
                sb.AppendLine($"WHERE");
            }

            foreach (var condition in _Conditions)
            {
                bool addParameter = true;
                switch (condition.operation)
                {
                    case "And":
                        sb.AppendLine($"AND");
                        addParameter = false;
                        break;
                    case "Or":
                        sb.AppendLine($"OR");
                        addParameter = false;
                        break;
                    case "Like":
                        sb.AppendLine($"(JSON_VALUE({condition.column},'${condition.key}') LIKE '%{{{parameters.Count()}}}%')");
                        break;
                    case "StartsWith":
                        sb.AppendLine($"(JSON_VALUE({condition.column},'${condition.key}') LIKE '{{{parameters.Count()}}}%')");
                        break;
                    case "EndsWith":
                        sb.AppendLine($"(JSON_VALUE({condition.column},'${condition.key}') LIKE '%{{{parameters.Count()}}}')");
                        break;
                    case "=":
                        sb.AppendLine($"(JSON_VALUE({condition.column},'${condition.key}') = '{{{parameters.Count()}}}')");
                        break;
                    case "!=":
                        sb.AppendLine($"(JSON_VALUE({condition.column},'${condition.key}') != '{{{parameters.Count()}}}')");
                        break;
                    case "ArrayContains":
                        sb.AppendLine($"('{parameters.Count()}' IN (SELECT value FROM OPENJSON({condition.column},'${condition.key}')))");
                        break;
                    case "ArrayItemStartsWith":
                        sb.AppendLine($"((SELECT count(value) FROM OPENJSON({condition.column},'${condition.key}') WHERE value LIKE '{{{parameters.Count()}}}%') > 0)");
                        break;
                    case "ArrayItemEndsWith":
                        sb.AppendLine($"((SELECT count(value) FROM OPENJSON({condition.column},'${condition.key}') WHERE value LIKE '%{{{parameters.Count()}}}') > 0)");
                        break;
                    default:
                        throw new Exception("Unsupported operation");
                }

                if(addParameter)
                {
                    parameters.Add(condition.param);
                }
            }

            var finalQuery = sb.ToString();

            var queryable = _dbSet.FromSqlRaw(finalQuery, parameters);

            //.NET Core 2.2
            //var queryable = _dbSet.FromSql(finalQuery, parameters);


            return queryable;
        }
    }

    public static class EntityPropertyExtensions
    {
        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(
       Expression<Func<TSource, TProperty>> propertyLambda)
        {
            Type type = typeof(TSource);

            MemberExpression member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }

        public static string GetColumnName(this IEntityType entityType, PropertyInfo propertyInfo)
        {
             return entityType.FindProperty(propertyInfo.Name).GetColumnName();
            //.NET Core 2.2 
            //return entityType.FindProperty(propertyInfo.Name).Relational().ColumnName;
        }
    }
}
