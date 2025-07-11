using DatabaseLayer.Attributes;
using System;
using System.Runtime.Serialization;

namespace Core.ApplicationModels.KironTestAPI.Tests
{
    [TableContract(Audited = true,
                   PrimaryKey = nameof(Id))]        
    public class TestTable2 : HasIdOnly, IEquatable<TestTable2>
    {
        [DataMember, ColumnContract]
        public override long Id { get; set; }

        [DataMember, ColumnContract(ForeignKeyType = typeof(TestTable),
                                    Required = true,
                                    Queryable = true)]
        public long IdTestTable { get; set; }

        [ColumnContract(nameof(TestTable),
                        nameof(IdTestTable),
                        nameof(TestTable.UserName))]
        public string UserName { get; set; }

        [ColumnContract(nameof(TestTable),
                        nameof(IdTestTable),
                        nameof(TestTable.Email))]
        public string Email { get; set; }

        [ColumnContract(Required = false, IsTimeStamp = true)]
        public DateTime CreatedDater { get; set; }

        public bool Equals(TestTable2 other)
        {
            if (other is null) return false;
            return IdTestTable == other.IdTestTable
                && string.Equals(Email, other.Email, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj) => Equals(obj as TestTable2);
        public override int GetHashCode()
        {
            unchecked
            {
                return (IdTestTable.GetHashCode() * 397)
                     ^ (Email?.ToLowerInvariant().GetHashCode() ?? 0);
            }
        }
    }
}
