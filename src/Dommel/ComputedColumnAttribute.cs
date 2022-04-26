using System;

namespace Dommel;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public class ComputedColumnAttribute : Attribute
{
    private string? alias;
    /// <summary>
    /// Constructs the ComputedColumnAttribute.
    /// </summary>
    /// <param name="expression">The sql expression for the computed column.</param>
    public ComputedColumnAttribute(string expression)
    {
        Expression = expression;
        alias = null;
    }

    public ComputedColumnAttribute(string expression, string alias)
    {
        Expression = expression;
        this.alias = alias;
    }
    
    public string Expression { get; }

    public string? Alias
    {
        get => alias;
        set
        {
            alias = value;
        }
    }
}