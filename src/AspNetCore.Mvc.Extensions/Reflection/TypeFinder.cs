using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspNetCore.Mvc.Extensions.Reflection
{
    public class TypeFinder : ITypeFinder
    {
        private readonly IAssemblyProvider _assemblyProvider;
        private Assembly[] _applicationAssemblies;

        public TypeFinder(IAssemblyProvider assemblyProvider)
        {
            _assemblyProvider = assemblyProvider;
        }

        #region protected 

        protected virtual Assembly[] ApplicationAssemblies
            => _applicationAssemblies ?? (_applicationAssemblies = _assemblyProvider.GetAssemblies().ToArray());

        /// <summary>
        ///     Does type implement generic?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="openGeneric"></param>
        /// <returns></returns>
        protected virtual bool DoesTypeImplementOpenGeneric(TypeInfo type, TypeInfo openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach (var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                {
                    if (!implementedInterface.GetTypeInfo().IsGenericType)
                        continue;

                    var isMatch = genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition());
                    return isMatch;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Methods

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, ApplicationAssemblies, onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies,
            bool onlyConcreteClasses = true)
        {
            var assignTypeFromInfo = assignTypeFrom.GetTypeInfo();
            var result = new List<Type>();
            try
            {
                foreach (var a in assemblies)
                {
                    foreach (var t in a.DefinedTypes)
                    {

                        if (assignTypeFromInfo.IsAssignableFrom(t) ||
                            (assignTypeFromInfo.IsGenericTypeDefinition && DoesTypeImplementOpenGeneric(t, assignTypeFromInfo)))
                        {
                            if (!t.IsInterface)
                            {
                                if (onlyConcreteClasses)
                                {
                                    if (t.IsClass && !t.IsAbstract)
                                        result.Add(t.AsType());
                                }
                                else
                                {
                                    result.Add(t.AsType());
                                }
                            }

                        }

                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                var msg = string.Empty;
                foreach (var e in ex.LoaderExceptions)
                    msg += e.Message + Environment.NewLine;

                var fail = new Exception(msg, ex);
                //Debug.WriteLine(fail.Message, fail);

                throw fail;
            }
            return result;
        }

        #endregion
    }
}