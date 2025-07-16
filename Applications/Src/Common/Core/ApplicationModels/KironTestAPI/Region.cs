using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(PrimaryKey= nameof(RegionId))]
    public class Region
    {
        [ColumnContract(Queryable= true)] 
        public int RegionId { get; set; }
        [ColumnContract(Length = 50, Queryable = true)] 
        public string RegionKey { get; set; }
        [ColumnContract(Length = 100, Queryable = true)] 
        public string RegionName { get; set; }
    }
}
