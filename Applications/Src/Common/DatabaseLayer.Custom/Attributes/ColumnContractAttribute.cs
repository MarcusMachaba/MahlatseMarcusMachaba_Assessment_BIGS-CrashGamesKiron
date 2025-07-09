using System;

namespace DatabaseLayer.Attributes
{
    public class ColumnContractAttribute : Attribute
    {
        public ColumnContractAttribute(params string[] joinedFrom) => JoinedFrom = joinedFrom;

        public object DefaultValue { get; set; }

        public Type ForeignKeyType { get; set; }

        public int Length { get; set; }

        public int Precision { get; set; }

        public int Scale { get; set; }

        public string[] JoinedFrom { get; private set; }

        public bool Required { get; set; }

        public bool Queryable { get; set; }

        public bool IsTimeStamp { get; set; }

        public string LengthString => Length != -1 ? Length.ToString() : "MAX";
    }
}
