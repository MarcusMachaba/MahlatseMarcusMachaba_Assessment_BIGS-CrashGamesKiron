using DatabaseLayer.Attributes;
using System;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(PrimaryKey= nameof(HolidayId))]
    public class Holiday
    {
        [ColumnContract(Queryable= true)] 
        public int HolidayId { get; set; }
        [ColumnContract(Queryable= true)] 
        public DateTime HolidayDate { get; set; }
        [ColumnContract(Length = 100, Queryable = true)] 
        public string Title { get; set; }
    }

}
