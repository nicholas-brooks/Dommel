using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Dommel;

/// <summary>
/// Helper class for retrieving type metadata to build SQL queries using the configured resolvers.
/// </summary>
public static class Resolvers
{
    private static readonly ConcurrentDictionary<string, TableName> TypeTableNameCache = new();
    private static readonly ConcurrentDictionary<string, IEnumerable<string>?> TypeSelectExpressionCache = new();
    private static readonly ConcurrentDictionary<string, string> ColumnNameCache = new();
    private static readonly ConcurrentDictionary<Type, ColumnPropertyInfo[]> TypeKeyPropertiesCache = new();
    private static readonly ConcurrentDictionary<Type, ColumnPropertyInfo[]> TypePropertiesCache = new();
    private static readonly ConcurrentDictionary<string, ForeignKeyInfo> TypeForeignKeyPropertyCache = new();

    /// <summary>
    /// Gets the key properties for the specified type, using the configured <see cref="IKeyPropertyResolver"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the key properties for.</param>
    /// <returns>The key properties for <paramref name="type"/>.</returns>
    public static ColumnPropertyInfo[] KeyProperties(Type type)
    {
        if (!TypeKeyPropertiesCache.TryGetValue(type, out var keyProperties))
        {
            keyProperties = DommelMapper.KeyPropertyResolver.ResolveKeyProperties(type);
            TypeKeyPropertiesCache.TryAdd(type, keyProperties);
        }

        DommelMapper.LogReceived?.Invoke($"Resolved property '{string.Join(", ", keyProperties.Select(p => p.Property.Name))}' as key property for '{type}'");
        return keyProperties;
    }

    /// <summary>
    /// Determine the left and right key column names based on the joining between sourceType and includeType.
    /// </summary>
    /// <param name="sqlBuilder">The SQL Builder instance.</param>
    /// <param name="sourceType">The type that represents the left side of the join expression</param>
    /// <param name="includeType">The type that represents the right-side of the join expression</param>
    /// <returns>Tuple with the column name of the left key and column name of the right key.</returns>
    public static (string leftKeyName, string rightKeyName) JoinProperties(ISqlBuilder sqlBuilder, Type sourceType, Type includeType)
    {
        var (leftKeyName, rightKeyName) = DommelMapper.MultiMapJoinResolver.ResolveJoinProperties(sqlBuilder, sourceType, includeType);
        DommelMapper.LogReceived?.Invoke($"Resolved join for {sourceType} -> {includeType} as {leftKeyName} = {rightKeyName}");
        return (leftKeyName, rightKeyName);
    }

/// <summary>
/// Gets the foreign key property for the specified source type and including type
/// using the configured <see cref="IForeignKeyPropertyResolver"/>.
/// </summary>
/// <param name="sourceType">The source type which should contain the foreign key property.</param>
/// <param name="includingType">The type of the foreign key relation.</param>
/// <param name="foreignKeyRelation">The foreign key relationship type.</param>
/// <returns>The foreign key property for <paramref name="sourceType"/> and <paramref name="includingType"/>.</returns>
public static PropertyInfo ForeignKeyProperty(Type sourceType, Type includingType, out ForeignKeyRelation foreignKeyRelation)
    {
        var key = $"{sourceType};{includingType}";
        if (!TypeForeignKeyPropertyCache.TryGetValue(key, out var foreignKeyInfo))
        {
            // Resolve the property and relation.
            var foreignKeyProperty = DommelMapper.ForeignKeyPropertyResolver.ResolveForeignKeyProperty(sourceType, includingType, out foreignKeyRelation);

            // Cache the info.
            foreignKeyInfo = new ForeignKeyInfo(foreignKeyProperty, foreignKeyRelation);
            TypeForeignKeyPropertyCache.TryAdd(key, foreignKeyInfo);
        }

        foreignKeyRelation = foreignKeyInfo.Relation;

        DommelMapper.LogReceived?.Invoke($"Resolved property '{foreignKeyInfo.PropertyInfo}' ({foreignKeyInfo.Relation}) as foreign key between '{sourceType}' and '{includingType}'");
        return foreignKeyInfo.PropertyInfo;
    }

    /// <summary>
    /// Gets the properties to be mapped for the specified type, using the configured
    /// <see cref="IPropertyResolver"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the properties from.</param>
    /// <returns>>The collection of to be mapped properties of <paramref name="type"/>.</returns>
    public static IEnumerable<ColumnPropertyInfo> Properties(Type type)
    {
        if (!TypePropertiesCache.TryGetValue(type, out var properties))
        {
            properties = DommelMapper.PropertyResolver.ResolveProperties(type).ToArray();
            TypePropertiesCache.TryAdd(type, properties);
        }

        return properties;
    }

    /// <summary>
    /// Gets the name of the table in the database for the specified type,
    /// using the configured <see cref="ITableNameResolver"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the table name for.</param>
    /// <param name="connection">The database connection instance.</param>
    /// <returns>The table name in the database for <paramref name="type"/>.</returns>
    public static TableName Table(Type type, IDbConnection connection) =>
        Table(type, DommelMapper.GetSqlBuilder(connection));

    /// <summary>
    /// Gets the name of the table in the database for the specified type,
    /// using the configured <see cref="ITableNameResolver"/>.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to get the table name for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>The table name in the database for <paramref name="type"/>.</returns>
    public static TableName Table(Type type, ISqlBuilder sqlBuilder)
    {
        var key = $"{sqlBuilder.GetType()}.{type}";
        if (!TypeTableNameCache.TryGetValue(key, out var name))
        {
            var resolvedName = DommelMapper.TableNameResolver.ResolveTableName(type);

            // Dots are used to define a schema which should be quoted separately
            if (resolvedName.Name.Contains('.'))
            {
                name = new TableName(
                        string.Join(".", resolvedName.Name.Split('.').Select(sqlBuilder.QuoteIdentifier)),
                        sqlBuilder.QuoteIdentifier(resolvedName.Alias));

            }
            else
            {
                name = new TableName(sqlBuilder.QuoteIdentifier(resolvedName.Name), sqlBuilder.QuoteIdentifier(resolvedName.Alias));
            }

            TypeTableNameCache.TryAdd(key, name);
        }

        DommelMapper.LogReceived?.Invoke($"Resolved table name '{name}' for '{type}'");
        return name;
    }

    /// <summary>
    /// Gets the select expression (e.g. list of fields or by default '*') portion of the sql statement for the 
    /// given type using the configured <see cref="ISelectExpressionResolver"/>.
    /// </summary>
    /// <param name="type">The type to generate the select expression for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>The select expression for the given type.</returns>
    public static string SelectExpression(Type type, ISqlBuilder sqlBuilder)
    {
        var key = $"{sqlBuilder.GetType()}.{type}";
        if (!TypeSelectExpressionCache.TryGetValue(key, out var fields))
        {
            fields = DommelMapper.SelectExpressionResolver.ResolveSelectExpression(type, sqlBuilder);
            TypeSelectExpressionCache.TryAdd(key, fields);
        }

        var select = fields != null ? string.Join(",", fields) : "*";

        DommelMapper.LogReceived?.Invoke($"Resolved select fields '{select}' for '{type}'");
        return select;
    }

    /// <summary>
    /// Gets the select expression (e.g. list of fields or by default '*') portion of the sql statement for the 
    /// given types using the configured <see cref="ISelectExpressionResolver"/>.
    /// </summary>
    /// <param name="types">The list of of types to generate the select expression for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>The select expression for the given Types.</returns>
    public static string SelectExpression(Type[] types, ISqlBuilder sqlBuilder)
    {
        var fields = new List<string>();
        foreach (var type in types)
        {
            var typeFields = SelectExpression(type, sqlBuilder);
            if (typeFields == "*")
                return "*"; // if any types are '*', we default to '*' for whole select expression.
            if (typeFields.Length > 0)
                fields.Add(typeFields);
        }

        return fields.Any() ? string.Join(",", fields) : "*";
    }

    /// <summary>
    /// Gets the name of the column in the database for the specified type,
    /// using the configured <see cref="IColumnNameResolver"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to get the column name for.</param>
    /// <param name="connection">The database connection instance.</param>
    /// <returns>The column name in the database for <paramref name="propertyInfo"/>.</returns>
    public static string Column(PropertyInfo propertyInfo, IDbConnection connection)
        => Column(propertyInfo, DommelMapper.GetSqlBuilder(connection));

    /// <summary>
    /// Gets the name of the column in the database for the specified type,
    /// using the configured <see cref="IColumnNameResolver"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to get the column name for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <returns>The column name in the database for <paramref name="propertyInfo"/>.</returns>
    public static string Column(PropertyInfo propertyInfo, ISqlBuilder sqlBuilder)
        => Column(propertyInfo, sqlBuilder, DommelMapper.IncludeTableNameInColumnName);

    /// <summary>
    /// Gets the name of the column in the database for the specified type,
    /// using the configured <see cref="IColumnNameResolver"/>.
    /// </summary>
    /// <param name="propertyInfo">The <see cref="PropertyInfo"/> to get the column name for.</param>
    /// <param name="sqlBuilder">The SQL builder instance.</param>
    /// <param name="includeTableName">Whether to include table name with the column name for unambiguity. E.g. <c>[Products].[Name]</c>.</param>
    /// <returns>The column name in the database for <paramref name="propertyInfo"/>.</returns>
    public static string Column(PropertyInfo propertyInfo, ISqlBuilder sqlBuilder, bool includeTableName = true)
    {
        var key = $"{sqlBuilder.GetType()}.{propertyInfo.ReflectedType}.{propertyInfo.Name}.{includeTableName}";
        if (!ColumnNameCache.TryGetValue(key, out var columnName))
        {
            columnName = sqlBuilder.QuoteIdentifier(DommelMapper.ColumnNameResolver.ResolveColumnName(propertyInfo));
            if (includeTableName && propertyInfo.ReflectedType?.IsDefined(typeof(CompilerGeneratedAttribute)) == false)
            {
                // Include the table name for unambiguity, except for anonymyes types e.g. x => new { x.Id, x.Name }
                var tableName = Table(propertyInfo.ReflectedType, sqlBuilder);
                columnName = $"{tableName.Alias}.{columnName}";
            }
            ColumnNameCache.TryAdd(key, columnName);
        }

        DommelMapper.LogReceived?.Invoke($"Resolved column name '{columnName}' for '{propertyInfo}'");
        return columnName;
    }

    private struct ForeignKeyInfo
    {
        public ForeignKeyInfo(PropertyInfo propertyInfo, ForeignKeyRelation relation)
        {
            PropertyInfo = propertyInfo;
            Relation = relation;
        }

        public PropertyInfo PropertyInfo { get; set; }

        public ForeignKeyRelation Relation { get; set; }
    }
}
