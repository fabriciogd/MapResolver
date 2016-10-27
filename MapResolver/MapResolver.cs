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

        private delegate TTarget Resolver<TSource, TTarget>(TSource source, TTarget target);

        private readonly IList<IResolver> _resolvers = new List<IResolver>();
        private readonly Dictionary<string, object> _delegates = new Dictionary<string, object>();

        public MapResolver()
        {
            this.AddResolver(new NestedResolver());
            this.AddResolver(new SimpleResolver());
        }

        public MapResolver AddResolver(IResolver propertyMapResolver)
        {
            this._resolvers.Add(propertyMapResolver);
            return this;
        }

        protected string GetMapKey<TSource, TTarget>()
        {
            return $"{typeof(TSource).FullName.Replace(".", "_")}_{typeof(TTarget).FullName.Replace(".", "_")}";
        }

        protected List<PropertyMap> ResolveProperties<TSource, TTarget>()
        {
            var targetProperties = typeof(TTarget).GetProperties().Where(t => t.CanWrite);
            var sourceProperties = typeof(TSource).GetProperties().Where(s => s.CanRead);

            List<PropertyMap> mappedProperties = new List<PropertyMap>();

            foreach (var propertyMapResolver in this._resolvers)
            {
                var propertiesResolved = propertyMapResolver.TryResolveProperties(targetProperties, sourceProperties);
                mappedProperties.AddRange(propertiesResolved);
            }

            return mappedProperties;
        }

        protected void MapProperties<TSource, TTarget>(IList<PropertyMap> maps, string key)
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

            this._delegates.Add(key, del);
        }

        protected void MapProperties<TSource, TTarget>(string key)
        {
            var maps = this.ResolveProperties<TSource, TTarget>();
            this.MapProperties<TSource, TTarget>(maps, key);
        }

        private Resolver<TSource, TTarget> GetDelegate<TSource, TTarget>()
        {
            var key = this.GetMapKey<TSource, TTarget>();

            if (!this._delegates.ContainsKey(key))
                this.MapProperties<TSource, TTarget>(key);

            return (Resolver<TSource, TTarget>)_delegates[key];
        }

        public void Map<TSource, TTarget>(TSource source, TTarget target)
        {
            var del = this.GetDelegate<TSource, TTarget>();
            del.Invoke(source, target);
        }

        public TTarget Map<TSource, TTarget>(TSource source)
            where TTarget : new()
        {
            TTarget target = new TTarget();
            this.Map(source, target);
            return target;
        }

        public void MapCollection<TSource, TTarget>(IEnumerable<TSource> source, IList<TTarget> target)
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

        public IEnumerable<TTarget> MapCollection<TSource, TTarget>(IEnumerable<TSource> source)
            where TTarget : new()
        {
            var del = this.GetDelegate<TSource, TTarget>();

            IEnumerator<TSource> sourceEnumerator = source.GetEnumerator();
            while (sourceEnumerator.MoveNext())
            {
                TTarget targetElement = new TTarget();
                yield return del(sourceEnumerator.Current, targetElement);
            }
        }

    }
}
