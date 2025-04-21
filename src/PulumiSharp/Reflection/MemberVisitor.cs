using System.Reflection;

namespace PulumiSharp.Reflection;

internal abstract class MemberVisitor
{
    public virtual void Visit(MemberInfo memberInfo)
    {
        switch (memberInfo)
        {
            case Type type:
                VisitType(type);
                break;
            case MethodInfo methodInfo:
                VisitMethod(methodInfo);
                break;
            case PropertyInfo propertyInfo:
                VisitProperty(propertyInfo);
                break;
        }
    }

    public virtual void VisitType(Type type)
    {

    }

    public virtual void VisitMethod(MethodInfo methodInfo)
    {

    }

    public virtual void VisitProperty(PropertyInfo propertyInfo)
    {
    }
}

public abstract class MemberVisitor<TResult>
{
    public virtual TResult? Visit(MemberInfo memberInfo)
    {
        switch (memberInfo)
        {
            case Type type:
                return VisitType(type);
            case MethodInfo methodInfo:
                return VisitMethod(methodInfo);
            case PropertyInfo propertyInfo:
                return VisitProperty(propertyInfo);
            default:
                return default;
        }
    }

    public virtual TResult? VisitType(Type type)
    {
        return default;
    }

    public virtual TResult? VisitMethod(MethodInfo methodInfo)
    {
        return default;
    }

    public virtual TResult? VisitProperty(PropertyInfo propertyInfo)
    {
        return default;
    }
}