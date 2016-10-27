namespace MapperResolver.Map
{
    using System.Reflection;
    using System.Reflection.Emit;

    public class SimpleMap : IMap
    {
        public PropertyInfo SourceProperty { get; set; }

        public PropertyInfo TargetProperty { get; set; }

        public void BuildIL(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldarg_0);

            il.Emit(OpCodes.Call, this.SourceProperty.GetGetMethod());
            il.Emit(OpCodes.Call, this.TargetProperty.GetSetMethod());
        }
    }
}
