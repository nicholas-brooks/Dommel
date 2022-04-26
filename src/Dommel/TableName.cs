namespace Dommel;

/// <summary>
/// Class representing the Table Name tuple (name and optional alias).
/// </summary>
public class TableName
{
    /// <summary>
    /// Constructor for TableName
    /// </summary>
    /// <param name="name">Name of the Table</param>
    /// <param name="alias">Alias for the table.  If null or empty will default to Name</param>
    public TableName(string name, string alias)
    {
        Name = name;
        Alias = string.IsNullOrEmpty(alias) ? name : alias;
    }
    /// <summary>
    /// Name of the Table.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Alias of the Table.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// String representation of TableName
    /// </summary>
    /// <returns>TableName representation</returns>
    public override string ToString()
    {
        return $"{Name} {Alias}";
    }
}
