using System.Collections.Generic;

namespace Core.ModelBase.Models
{
    public class ServiceResponseModel<T>
    {
        public ServiceResponseModel()
        {
            Errors = new List<string>();
        }

        public ServiceResponseModel(T data)
        {
            Data = data;
        }
        public bool ContainsError { get { return Errors != null && Errors.Count > 0; } }
        public bool ContainsData { get { return Data != null; } }
        public T Data { get; set; }
        public List<string> Errors { get; set; }
    }
}
