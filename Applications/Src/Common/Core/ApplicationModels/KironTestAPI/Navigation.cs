using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(Audited = true, PartitionOn = nameof(Navigation.ID), PrimaryKey = nameof(Navigation.ID))]
    public class Navigation
    {
        [ColumnContract(Required= true, Queryable= true)]
        public int ID { get; set; }
        [ColumnContract(Length= 50, Required= true, Queryable= true)]
        [StringLength(50)]
        public string Text { get; set; }
        // ParentID in SQL is NOT NULL but -1 means “no parent”
        [ColumnContract(Required= true, Queryable= true)]
        public int ParentID { get; set; }
        [ForeignKey(nameof(ParentID))]
        public Navigation Parent { get; set; }
        public ICollection<Navigation> Children { get; set; }
    }
}
