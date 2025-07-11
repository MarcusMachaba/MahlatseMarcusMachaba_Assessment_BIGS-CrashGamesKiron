using Core;
using DatabaseLayer.Attributes;

namespace DebugTester.DataAccessLayer.Tests.Models
{
    [TableContract(PrimaryKey = nameof(Bank.Id))]
    public class Bank: HasIdOnly
    {
        [ColumnContract(Length = 100)]
        public string Name { get; set; }

        [ColumnContract(Length = 1050)]
        public string SecondName { get; set; }
    }
}
