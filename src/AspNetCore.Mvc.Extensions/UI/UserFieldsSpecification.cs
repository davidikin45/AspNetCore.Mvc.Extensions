using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AspNetCore.Mvc.Extensions.UI
{
    public abstract class UserFieldsSpecification
    {
        public Type Type { get; }
        public string Fields { get; }

        public static UserFieldsSpecification<T> Create<T>(string fields)
        {
            return new UserFieldsSpecification<T>(fields);

        }

        public static UserFieldsSpecification Create(Type type, string fields)
        {
            return (UserFieldsSpecification)typeof(UserFieldsSpecification).GetMethod("Create", new Type[] { typeof(string) }).MakeGenericMethod(type).Invoke(null, new object[] { fields });
        }

        protected internal UserFieldsSpecification(Type type, string fields)
        {
            Type = type;
            Fields = fields;
            IsValid = UIHelper.ValidFieldsFor(type, fields);
        }

        public bool IsValid { get; }

        public abstract ExpandoObject ShapeData(object source);

        public abstract IEnumerable<ExpandoObject> ShapeData(IEnumerable<object> source);
    }

    public class UserFieldsSpecification<T> : UserFieldsSpecification
    {

        //query string
        protected internal UserFieldsSpecification(string fields)
            :base(typeof(T), fields)
        {

        }

        public ExpandoObject ShapeData(T source)
        {
            if (!IsValid)
                throw new InvalidOperationException();

            return UIHelper.ShapeData<T>(source, Fields);
        }

        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> source)
        {
            if (!IsValid)
                throw new InvalidOperationException();

            return UIHelper.ShapeListData<T>(source, Fields);
        }

        public override ExpandoObject ShapeData(object source)
        {
            if (source is T castSource)
            {
                return ShapeData(castSource);
            }
            else
            {
                throw new Exception("Invalid object type");
            }
        }

        public override IEnumerable<ExpandoObject> ShapeData(IEnumerable<object> source)
        {
            if (source is IEnumerable<T> castSource)
            {
                return ShapeData(castSource);
            }
            else
            {
                throw new Exception("Invalid object type");
            }
        }
    }
}
