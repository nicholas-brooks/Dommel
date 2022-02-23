using System;
using System.Collections.Generic;

namespace Dommel;

/// <summary>
/// Defines methods for resolving the select expression for entities.
/// Custom implementations can be registered with <see cref="DommelMapper.SetSelectExpressionResolver(ISelectExpressionResolver)"/>.
/// </summary>
public interface ISelectExpressionResolver
{
    /// <summary>
    /// Resolves the properties for the specified type into a select expression.  If Dommel should select all, return null.
    /// </summary>
    /// <param name="type">The type to resolve the key properties for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>A collection of field names for <paramref name="type"/> or null to indicate to select '*'.</returns>
    IEnumerable<string>? ResolveSelectExpression(Type type, ISqlBuilder sqlBuilder);
}
