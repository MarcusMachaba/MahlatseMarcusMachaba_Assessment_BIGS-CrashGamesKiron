using Core;
using DatabaseLayer.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTester.DataAccessLayer.Tests.Models
{
    [TableContract(PrimaryKey = nameof(Branch.Id))]
    public class Branch: HasIdOnly
    {
        [ColumnContract(ForeignKeyType = typeof(Bank), Queryable = true)]
        public long IdBank { get; set; }
        [ColumnContract(Length = 10)]
        public string BranchName { get; set; }
    }
}
