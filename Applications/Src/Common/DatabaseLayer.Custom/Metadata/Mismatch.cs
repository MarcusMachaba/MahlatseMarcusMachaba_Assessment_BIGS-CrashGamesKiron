namespace DatabaseLayer.Metadata
{
    public class Mismatch
    {
        public string ModelValue { get; private set; }

        public string DbValue { get; private set; }

        public Mismatch(string modelValue, string dbValue)
        {
            this.ModelValue = modelValue;
            this.DbValue = dbValue;
        }
    }
}
