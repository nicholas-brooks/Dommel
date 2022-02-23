using System;
using System.Collections.Generic;

namespace Dommel;

/// <summary>
/// Implements the <see cref="ISelectExpressionResolver"/> interface by returning null to indicate
/// to the call to use '*' for the select expression.
/// </summary>
public class DefaultSelectExpressionResolver : ISelectExpressionResolver
{
    /// <summary>
    /// Returns an null to indicate to use select '*'.
    /// </summary>
    /// <param name="type">The type to resolve the key properties for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>null to indicate for the caller to use '*'</returns>
    public IEnumerable<string>? ResolveSelectExpression(Type type, ISqlBuilder sqlBuilder)
    {
        return null;
    }
}
