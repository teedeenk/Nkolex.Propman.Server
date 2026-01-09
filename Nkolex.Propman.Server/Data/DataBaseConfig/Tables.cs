using Nkolex.Propman.Server.Abstractions;

namespace Nkolex.Propman.Server.Data.DataBaseConfig
{
    public class Tables : ITables
    {
        public List<string> TableNames { get; set; } = [];

        public string ResolveNameFor(Type entityType)
        {
            var typeName = entityType.Name;

            if (typeName.Length > 1 && typeName[0] == 'I' && char.IsUpper(typeName[1]))
            {
                typeName = typeName.Substring(1);
            }

            var match = TableNames.FirstOrDefault(t =>
                string.Equals(t, typeName, StringComparison.OrdinalIgnoreCase));

            return match ?? typeName;
        }
    }
}
