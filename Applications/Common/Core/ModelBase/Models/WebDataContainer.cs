namespace Core.ModelBase.Models
{
    public class WebDataContainer<T> : WebData where T : class
    {
        public T Data { get; set; }
    }
}
