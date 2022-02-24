using System;

namespace Dommel;

/// <summary>
/// Defines method of resolving Join Keys and type.
/// </summary>
public interface IMultiMapJoinResolver
{
    /// <summary>
    /// Resolves the join requirements.
    /// </summary>
    /// <param name="sqlBuilder">SqlBuilder</param>
    /// <param name="sourceType">The Type of the Source of the Join.</param>
    /// <param name="includingType">The Type that will be joined to the Source.</param>
    /// <returns></returns>
    (string leftKeyName, string rightKeyName) ResolveJoinProperties(ISqlBuilder sqlBuilder, Type sourceType, Type includingType);
}
