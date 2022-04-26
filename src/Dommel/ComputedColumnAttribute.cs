using System;

namespace Dommel;

/// <summary>
/// Define the property as a Computed Column that is calculated from a SQL Expression.
/// </summary>
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

    /// <summary>
    /// Constructs the ComputedColumnAttribute with an Alias.
    /// </summary>
    /// <param name="expression">The sql expression for the computed column.</param>
    /// <param name="alias">The alias to use for this column</param>
    public ComputedColumnAttribute(string expression, string alias)
    {
        Expression = expression;
        this.alias = alias;
    }
    
    /// <summary>
    /// SQL Expression to use for this Computed Column
    /// </summary>
    public string Expression { get; }

    /// <summary>
    /// Optional Alias name to use for this Computed Column
    /// </summary>
    public string? Alias
    {
        get => alias;
        set
        {
            alias = value;
        }
    }
}