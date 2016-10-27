namespace MapperResolver.Map
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    public class NestedMap : IMap
    {
        public List<PropertyInfo> NestedProperties { get; set; }

        public PropertyInfo TargetProperty { get; set; }

        public NestedMap()
        {
            NestedProperties = new List<PropertyInfo>();
        }

        public void BuildIL(ILGenerator il)
        {
            Label endIfLabel = il.DefineLabel();

            for (int i = 0; i < this.NestedProperties.Count - 1; i++)
            {

                il.Emit(OpCodes.Ldarg_0);

                for (int x = 0; x <= i; x++)
                {
                    il.Emit(OpCodes.Call, this.NestedProperties[x].GetGetMethod());
                }

                il.Emit(OpCodes.Brfalse, endIfLabel);

            }

            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);

            foreach (PropertyInfo propertyInfo in this.NestedProperties)
            {
                il.Emit(OpCodes.Call, propertyInfo.GetGetMethod());
            }

            il.Emit(OpCodes.Call, this.TargetProperty.GetSetMethod());

            il.MarkLabel(endIfLabel);

        }
    }
}
