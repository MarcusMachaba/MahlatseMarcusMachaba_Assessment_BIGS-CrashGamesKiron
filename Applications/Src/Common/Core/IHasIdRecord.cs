using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core
{
    /// <summary>
    /// IHasIdRecord (Comes with Id, Created-Date, Updated-Date & intVersion)
    /// </summary>
    #region IHasIdRecord
    public interface IHasIdRecord : IHasId
    {
        DateTime Updated { get; set; }
        int Version { get; set; }
    }
    [DataContract]
    public abstract class HasIdRecord : HasId, IHasIdRecord
    {
        public HasIdRecord() : base()
        {
            Updated = DateTime.Now;
            Version = 1;
        }

        [DataMember, ColumnContract]
        public DateTime Updated { get; set; }

        [DataMember, ColumnContract]
        public int Version { get; set; }
    }
    #endregion

    /// <summary>
    /// IHasGuidIdRecord (Comes with Id, Created-Date & LastUpdate-Date)
    /// </summary>
    #region IHasGuidIdRecord
    public interface IHasGIdRecord : IHasGId
    {
        DateTime Created { get; set; }
        DateTime LastUpdate { get; set; }
    }
    [DataContract]
    public abstract class HasGIdRecord : HasGId, IHasGIdRecord
    {
        public HasGIdRecord()
        {
            Created = DateTime.Now;
            LastUpdate = DateTime.Now;
        }
        [DataMember, ColumnContract]
        public DateTime Created { get; set; }
        [DataMember, ColumnContract]
        public DateTime LastUpdate { get; set; }
    }
    #endregion
}
