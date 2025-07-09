using DatabaseLayer.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Core
{
    /// <summary>
    /// IHasIdOnly (Comes with Id only)
    /// </summary>
    #region IHasIdOnly
    public interface IHasIdOnly
    {
        long Id { get; set; }
    }
    [DataContract]
    public class HasIdOnly : IHasIdOnly
    {
        [DataMember, BsonId, ColumnContract]
        public long Id { get; set; }
    }
    #endregion

    /// <summary>
    /// IHasId (Comes with Id & Created-Date)
    /// </summary>
    #region IHasId
    public interface IHasId : IHasIdOnly
    {
        DateTime CreationDate { get; set; }
    }

    [DataContract]
    public abstract class HasId : HasIdOnly, IHasId
    {
        public HasId()
        {
            CreationDate = DateTime.Now;
        }
        //[DataType(DataType.Date)]
        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DataMember, Display(Name = "Creation Date"), DataType(DataType.Date), ColumnContract]
        public DateTime CreationDate { get; set; }
    }
    #endregion

    /// <summary>
    /// IHasGuidIdOnly (Comes with Guid Id only)
    /// </summary>
    #region IHasGuidIdOnly
    public interface IHasGId
    {
        Guid Id { get; set; }
    }
    [DataContract]
    public abstract class HasGId : IHasGId
    {
        public HasGId()
        {
            Id = Guid.NewGuid();
        }
        [DataMember, BsonId, ColumnContract]
        public Guid Id { get; set; }
    }
    #endregion
}
