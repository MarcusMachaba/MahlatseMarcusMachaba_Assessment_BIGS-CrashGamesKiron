using DatabaseLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DatabaseLayer.Metadata
{
    public class MetadataStore
    {
        public MetadataStore(BaseDataProvider dataProvider)
        {
            this.Tables = new List<TableMetadata>();
            foreach (PropertyInfo property in dataProvider.GetType().GetProperties())
            {
                if (property.PropertyType.GetInterface("IDataObjectInterface") != (Type)null)
                    this.Tables.Add(((IDataObjectInterface)(property.GetValue((object)dataProvider) ?? throw new InvalidOperationException("Property " + ((MemberInfo)property).Name + " was not initialized in the data container. Please initialize the object in the constructor."))).Metadata);
            }
        }

        public List<TableMetadata> Tables { get; private set; }
    }
}
