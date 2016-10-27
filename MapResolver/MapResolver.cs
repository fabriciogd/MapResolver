namespace MapperResolver
{
    using Map;
    using Resolver;
    using Resolvers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;

    public class MapResolver
    {
        private static volatile MapResolver instance;
        private static object syncRoot = new Object();

        public static MapResolver Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new MapResolver();
                    }
                }

                return instance;
            }
        }

        private IList<IResolver> _propertyMapResolvers;
        private delegate TTarget Resolver<TSource, TTarget>(TSource source, TTarget target);
        private readonly Dictionary<string, object> _delegatesDictionary = new Dictionary<string, object>();

        public MapResolver()
        {
            this._propertyMapResolvers = new List<IResolver>();
            this.AddResolver(new SimpleResolver());
            this.AddResolver(new NestedResolver());
        }

        public MapResolver AddResolver(IResolver propertyMapResolver)
        {
            this._propertyMapResolvers.Add(propertyMapResolver);
            return this;
        }

        protected string GetMapKey<TSource, TTarget>()
        {
            var className = "Copy_";
            className += typeof(TSource).FullName.Replace(".", "_");
            className += "_";
            className += typeof(TTarget).FullName.Replace(".", "_");

            return className;
        }

        protected List<IMap> GetMapProperties<TSource, TTarget>()
        {
            var targetProperties = typeof(TTarget).GetProperties().Where(t => t.CanWrite);
            var sourceProperties = typeof(TSource).GetProperties().Where(s => s.CanRead);

            List<IMap> mappedProperties = new List<IMap>();

            foreach (var propertyMapResolver in this._propertyMapResolvers)
            {
                var propertiesResolved = propertyMapResolver.TryResolveProperties(targetProperties, sourceProperties);
                mappedProperties.AddRange(propertiesResolved);
            }

            return mappedProperties;
        }

        protected void MapTypes<TSource, TTarget>(IList<IMap> maps, string key)
        {
            var source = typeof(TSource);
            var target = typeof(TTarget);

            var dynamicMethod = new DynamicMethod("DynMethod", target, new Type[] { source, target });
            ILGenerator genMethod = dynamicMethod.GetILGenerator();

            foreach (var map in maps)
            {
                map.BuildIL(genMethod);
            }

            genMethod.Emit(OpCodes.Ldarg_1);
            genMethod.Emit(OpCodes.Ret);

            var del = dynamicMethod.CreateDelegate(typeof(Resolver<TSource, TTarget>));

            this._delegatesDictionary.Add(key, del);
        }

        protected void MapTypes<TSource, TTarget>(string key)
        {
            var maps = this.GetMapProperties<TSource, TTarget>();
            this.MapTypes<TSource, TTarget>(maps, key);
        }

        private bool DelegateKeyExists(string key)
        {
            return this._delegatesDictionary.ContainsKey(key);
        }

        private Resolver<TSource, TTarget> GetDelegate<TSource, TTarget>()
        {
            var key = this.GetMapKey<TSource, TTarget>();

            if (!this.DelegateKeyExists(key))
                this.MapTypes<TSource, TTarget>(key);

            return (Resolver<TSource, TTarget>)_delegatesDictionary[key];
        }


        public void Transform<TSource, TTarget>(TSource source, TTarget target)
        {
            var del = this.GetDelegate<TSource, TTarget>();
            del.Invoke(source, target);
        }

        public void Transform<TSource, TTarget>(IEnumerable<TSource> source, IList<TTarget> target)
            where TTarget : new()
        {
            var del = this.GetDelegate<TSource, TTarget>();

            IEnumerator<TSource> sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext())
            {
                TTarget targetElement = new TTarget();
                del(sourceEnumerator.Current, targetElement);
                target.Add(targetElement);
            }
        }
    }
}
