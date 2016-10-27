namespace MapperResolver.Resolver
{
    using Map;
    using MapperResolver.Resolvers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class NestedResolver : IResolver
    {
        public IEnumerable<PropertyMap> TryResolveProperties(IEnumerable<PropertyInfo> targetProperties, IEnumerable<PropertyInfo> sourceProperties)
        {
            List<NestedPropertyMap> properties = new List<NestedPropertyMap>();

            foreach (var targetProperty in targetProperties)
            {
                if (targetProperty.Name.Contains("_"))
                {
                    NestedPropertyMap map = new NestedPropertyMap() { TargetProperty = targetProperty };
                    string[] memberNames = targetProperty.Name.Split('_');


                    foreach (string memberName in memberNames)
                    {
                        PropertyInfo propertyMember = sourceProperties.SingleOrDefault(s => s.Name == memberName);
                        if (propertyMember != null)
                        {
                            map.NestedProperties.Add(propertyMember);
                            sourceProperties = propertyMember.PropertyType.GetProperties();
                        }
                    }

                    if (memberNames.Length == map.NestedProperties.Count)
                        properties.Add(map);
                }
            }

            return properties;
        }
    }
}
