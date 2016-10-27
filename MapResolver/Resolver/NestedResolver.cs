namespace MapperResolver.Resolver
{
    using Map;
    using MapperResolver.Resolvers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class NestedResolver : IResolver
    {
        public IEnumerable<IMap> TryResolveProperties(IEnumerable<PropertyInfo> targetProperties, IEnumerable<PropertyInfo> sourceProperties)
        {
            List<NestedMap> properties = new List<NestedMap>();

            foreach (var remainingProperty in targetProperties)
            {
                if (remainingProperty.Name.Contains("_"))
                {
                    NestedMap nestedPropertyMap = new NestedMap() { TargetProperty = remainingProperty };
                    string[] nestedMemberNames = remainingProperty.Name.Split('_');

                    IEnumerable<PropertyInfo> propertiesInScope = sourceProperties;

                    foreach (string memberName in nestedMemberNames)
                    {
                        PropertyInfo propertyMember = propertiesInScope.SingleOrDefault(s => s.Name == memberName);
                        if (propertyMember != null)
                        {
                            nestedPropertyMap.NestedProperties.Add(propertyMember);
                            propertiesInScope = propertyMember.PropertyType.GetProperties();
                        }
                    }

                    if (nestedMemberNames.Length == nestedPropertyMap.NestedProperties.Count)
                        properties.Add(nestedPropertyMap);
                }
            }

            return properties;
        }
    }
}
