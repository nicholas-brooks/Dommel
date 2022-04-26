namespace Dommel;

public class TableName
{
    public TableName(string name, string alias)
    {
        Name = name;
        Alias = string.IsNullOrEmpty(alias) ? name : alias;
    }
    public string Name { get; }
    public string Alias { get; }

    public override string ToString()
    {
        return $"{Name} {Alias}";
    }
}
