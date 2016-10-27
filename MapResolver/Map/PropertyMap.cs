namespace MapperResolver.Map
{
    using System.Reflection;
    using System.Reflection.Emit;

    public abstract class PropertyMap
    {
        public PropertyInfo TargetProperty { get; set; }

        public abstract void BuildIL(ILGenerator il);
    }
}
