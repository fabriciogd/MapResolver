namespace MapperResolver.Resolvers
{
    using MapperResolver.Map;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class SimpleResolver : IResolver
    {
        public IEnumerable<IMap> TryResolveProperties(IEnumerable<PropertyInfo> targetProperties, IEnumerable<PropertyInfo> sourceProperties)
        {
            return (from s in sourceProperties
                    from t in targetProperties
                    where s.Name == t.Name &&
                          s.PropertyType == t.PropertyType
                    select new SimpleMap
                    {
                        SourceProperty = s,
                        TargetProperty = t
                    }).ToList();
        }
    }
}
