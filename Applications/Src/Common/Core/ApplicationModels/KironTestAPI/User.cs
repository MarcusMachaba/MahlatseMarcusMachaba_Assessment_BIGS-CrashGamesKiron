using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(Audited = true, PartitionOn = nameof(User.Id), PrimaryKey = nameof(User.Id))]
    public class User : HasId, IEquatable<User>
    {
        [ColumnContract(Length = 255, Required = true, Queryable = true)]
        public string UserName { get; set; } = string.Empty;
        [ColumnContract(Length = 255, Required = true, Queryable = true)]
        public string EmailAddress { get; set; }
        [ColumnContract(Length = 5000, Required = true)]
        public string PasswordHash { get; set; } = string.Empty;
        public string Salt { get; set; }
        public bool Equals(User other)
        {
            if (other is null) return false;
            return EmailAddress.Equals(other.EmailAddress, StringComparison.OrdinalIgnoreCase);
        }
        public override bool Equals(object obj) => Equals(obj as User);
        public override int GetHashCode() => EmailAddress?.ToLowerInvariant().GetHashCode() ?? 0;
    }
}
