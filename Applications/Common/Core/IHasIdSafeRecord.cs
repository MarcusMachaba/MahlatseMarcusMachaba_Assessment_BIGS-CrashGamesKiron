using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core
{
    /// <summary>
    /// IHasIdSafeRecord
    /// </summary>
    /// 
    public interface IHasIdSafeRecord : IHasIdRecord
    {
        long UpdatedById { get; set; }
        bool Archived { get; set; }
    }
    [DataContract]
    public abstract class HasIdSafeRecord : HasIdRecord, IHasIdSafeRecord
    {
        public HasIdSafeRecord() : base()
        {
            Archived = false;
        }
        [DataMember]
        public long UpdatedById { get; set; }

        [DataMember]
        public bool Archived { get; set; }
    }


    ///////////////////////////////////////////////////////


    public interface IHasGIdSafeRecord : IHasGIdRecord
    {
        bool Archived { get; set; }
        DateTime Updated { get; set; }
        Guid UpdatedById { get; set; }
    }
    [DataContract]
    public abstract class HasGIdSafeRecord : HasGIdRecord, IHasGIdSafeRecord
    {
        public HasGIdSafeRecord()
        {
            Updated = DateTime.Now;
        }
        public bool Archived { get; set; }
        public DateTime Updated { get; set; }
        public Guid UpdatedById { get; set; }
    }
}
}
