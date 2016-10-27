namespace MapperResolver.Map
{
    using System.Reflection;
    using System.Reflection.Emit;

    public interface IMap
    {
        PropertyInfo TargetProperty { get; set; }

        void BuildIL(ILGenerator il);
    }
}
