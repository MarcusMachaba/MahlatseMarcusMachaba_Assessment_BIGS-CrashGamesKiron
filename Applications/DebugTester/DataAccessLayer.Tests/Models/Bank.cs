using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTester.DataAccessLayer.Tests.Models
{
    [TableContract(PrimaryKey = nameof(Bank.IdBank))]
    public class Bank
    {
        [ColumnContract]
        public int IdBank { get; set; }
        [ColumnContract(Length = 100)]
        public string Name { get; set; }

        [ColumnContract(Length = 100)]
        public string SecondName { get; set; }
    }
}
