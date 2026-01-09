using CsvHelper.Configuration;

namespace Nkolex.Propman.Server.Models.Configs
{
    public sealed class StatementLineMap : ClassMap<StatementLine>
    {
        public StatementLineMap()
        {
            Map(m => m.Date).Index(0);
            Map(m => m.Description).Index(1);
            Map(m => m.Amount).Index(2);
        }
    }
}
