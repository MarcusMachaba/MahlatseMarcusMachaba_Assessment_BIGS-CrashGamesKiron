namespace DatabaseLayer.Dapper.QueryModel
{
    public class QueryPropertyPair
    {
        public QueryPropertyPair(string name, object value) : this(name, value, PropertyPairCoparerEnum.Equals) { }
        public QueryPropertyPair(string name, object value, PropertyPairCoparerEnum comparer) :
            this(name, value, comparer, null)
        { }
        public QueryPropertyPair(string name, object value, PropertyPairCoparerEnum comparer, string customValueName)
        {
            Name = name;
            Value = value;
            Comparer = comparer;
            CustomValueName = customValueName;
        }
        public string Name { get; set; }
        public string CustomValueName { get; set; }
        public object Value { get; set; }
        public PropertyPairCoparerEnum Comparer { get; set; }

        public static QueryPropertyPair New(string name, object value, PropertyPairCoparerEnum comparer = PropertyPairCoparerEnum.Equals, string customValueName = null)
        {
            return new QueryPropertyPair(name, value, comparer, customValueName);
        }
    }
}
