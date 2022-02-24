using System;
using System.Linq;

namespace Dommel;

/// <summary>
/// Default implmentation of the MultiMap Join Resolver.
/// </summary>
public class DefaultMultiMapJoinResolver : IMultiMapJoinResolver
{
    /// <summary>
    /// The default implementation of the MultiMap join.  Uses the ForeignKeyPropertyResolver to do
    /// most of the work.
    /// </summary>
    /// <param name="sqlBuilder">The SQL Builder instance.</param>
    /// <param name="sourceType">The type that represents the left side of the join expression</param>
    /// <param name="includingType">The type that represents the right-side of the join expression</param>
    /// <returns>Tuple with the column name of the left key and column name of the right key.</returns>
    /// <exception cref="InvalidOperationException">Thrown if There is an unknown ForeignKeyRelation enume value.</exception>
    public (string leftKeyName, string rightKeyName) ResolveJoinProperties(ISqlBuilder sqlBuilder, Type sourceType, Type includingType)
    {
        // Determine the foreign key and the relationship type.
        var foreignKeyProperty = Resolvers.ForeignKeyProperty(sourceType, includingType, out var relation);
        var foreignKeyPropertyName = Resolvers.Column(foreignKeyProperty, sqlBuilder);

        switch (relation)
        {
            case ForeignKeyRelation.OneToOne:
                var foreignKeyTableKeyColumName = Resolvers.Column(Resolvers.KeyProperties(includingType).Single().Property, sqlBuilder);
                return (foreignKeyPropertyName, foreignKeyTableKeyColumName);
            case ForeignKeyRelation.OneToMany:
                var sourceKeyColumnName = Resolvers.Column(Resolvers.KeyProperties(sourceType).Single().Property, sqlBuilder);
                return (sourceKeyColumnName, foreignKeyPropertyName);
            default:
                throw new InvalidOperationException($"Unhandled enum value of {relation} for {typeof(ForeignKeyRelation)}");
        }
    }
}
