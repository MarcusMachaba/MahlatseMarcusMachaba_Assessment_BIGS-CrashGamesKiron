using System;

namespace DatabaseLayer.Attributes
{
    public class TableContractAttribute : Attribute
    {
        public TableContractAttribute() => this.FileGroup = "PRIMARY";

        public string FileGroup { get; set; }

        public string PartitionOn { get; set; }

        public bool Audited { get; set; }

        public string PrimaryKey { get; set; }
        public bool Identity { get; set; } = true;
    }
}
