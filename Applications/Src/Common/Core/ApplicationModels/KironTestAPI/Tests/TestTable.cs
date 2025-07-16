using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Serialization;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI.Tests
{
    [TableContract(Audited = true, PartitionOn = nameof(Id), PrimaryKey = nameof(Id))]      
    public class TestTable : HasId, IEquatable<TestTable>
    {
        [DataMember, ColumnContract]
        public override long Id { get; set; }

        [ColumnContract(Length = 255, Required = true, Queryable = true)]
        public string UserName { get; set; }

        [ColumnContract(Length = 20, DefaultValue = "LIGHT")]
        public string DefaultTheme { get; set; }

        [ColumnContract]
        public CommunicationMethods TwoFactorType { get; set; }

        [ColumnContract(Length = 255, Required = true, Queryable = true)]
        public string Email { get; set; }

        [ColumnContract(DefaultValue = false)]
        public bool IsActive { get; set; }

        public bool Equals(TestTable other)
        {
            if (other is null) return false;
            return Email.Equals(other.Email, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj) => Equals(obj as TestTable);
        public override int GetHashCode() => Email?.ToLowerInvariant().GetHashCode() ?? 0;
    }
}
