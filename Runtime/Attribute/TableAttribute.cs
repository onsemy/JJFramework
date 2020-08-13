namespace JJFramework.Runtime.Attribute
{
    public class TableAttribute : System.Attribute
    {
        public readonly string name;

        public TableAttribute(string inName)
        {
            name = inName;
        }
    }
}
