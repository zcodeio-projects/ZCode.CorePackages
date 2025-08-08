namespace ZCode.Core.Domain.Extensions;

public static class TypeExtensions
{
    /// <summary>
    /// Checks if a type is of the specified type or implements it
    /// </summary>
    /// <param name="type">Type to check</param>
    /// <param name="checkedType">Type to check against</param>
    /// <returns>True if the type matches or implements the checked type</returns>
    public static bool IsOrImplements(this Type type, Type checkedType)
    {
        if (checkedType.IsGenericTypeDefinition)
            return type.GetInterfaces().Any(i =>
                i.IsGenericType && i.GetGenericTypeDefinition() == checkedType);
        
        return checkedType.IsAssignableFrom(type);
    }
}
