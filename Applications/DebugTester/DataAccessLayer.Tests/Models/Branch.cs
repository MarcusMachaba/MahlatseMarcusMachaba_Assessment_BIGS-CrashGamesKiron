using DatabaseLayer.Attributes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTester.DataAccessLayer.Tests.Models
{
    [TableContract(PrimaryKey = nameof(Branch.IdBranch))]
    public class Branch
    {
        [ColumnContract]
        public int IdBranch { get; set; }
        [ColumnContract(ForeignKeyType = typeof(Bank), Queryable = true)]
        public int IdBank { get; set; }
        [ColumnContract(Length = 10)]
        public string BranchName { get; set; }
    }
}
