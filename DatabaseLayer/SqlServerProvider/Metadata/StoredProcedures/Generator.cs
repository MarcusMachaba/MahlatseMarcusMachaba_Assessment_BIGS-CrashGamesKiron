using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures.Templates;
using System.Collections.Generic;

namespace DatabaseLayer.SqlServerProvider.Metadata.StoredProcedures
{
    public static class Generator
    {
        public static List<IStoredProcedure> GetStoredProcedures(TableMetadata table, BaseDataProvider provider)
        {
            DeleteTemplate deleteTemplate = new DeleteTemplate();
            deleteTemplate.Populate(table, provider);
            InsertTemplate insertTemplate = new InsertTemplate();
            insertTemplate.Populate(table, provider);
            UpdateTemplate updateTemplate = new UpdateTemplate();
            updateTemplate.Populate(table, provider);
            RetrieveTemplate retrieveTemplate = new RetrieveTemplate();
            retrieveTemplate.Populate(table, provider);

            return new List<IStoredProcedure>()
            {
                (IStoredProcedure) new BasicStoredProcedure(deleteTemplate.Name, deleteTemplate.Template),
                (IStoredProcedure) new BasicStoredProcedure(insertTemplate.Name, insertTemplate.Template),
                (IStoredProcedure) new BasicStoredProcedure(updateTemplate.Name, updateTemplate.Template),
                (IStoredProcedure) new BasicStoredProcedure(retrieveTemplate.Name, retrieveTemplate.Template)
            };
        }
    }
}
