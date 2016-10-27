namespace MapperResolver.Resolvers
{
    using MapperResolver.Map;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IResolver
    {
        IEnumerable<PropertyMap> TryResolveProperties(IEnumerable<PropertyInfo> targetProperties, IEnumerable<PropertyInfo> sourceProperties);
    }
}
