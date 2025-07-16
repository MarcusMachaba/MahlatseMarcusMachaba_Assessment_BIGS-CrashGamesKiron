using DatabaseLayer.Attributes;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(PrimaryKey= nameof(RegionId), Identity = false)]
    public class RegionHoliday
    {
        [ColumnContract(Required = true, Queryable = true, ForeignKeyType = typeof(Region))]
        public int RegionId { get; set; }
        [ColumnContract(Required = true, Queryable = true, ForeignKeyType = typeof(Holiday))]
        public int HolidayId { get; set; }
    }

}
