using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ModelBase.Models
{
    public class DataContainerVM<T>
    {
        public DataContainerVM(T data)
        {
            Data = data;
        }
        public T Data { get; set; }
    }
}
