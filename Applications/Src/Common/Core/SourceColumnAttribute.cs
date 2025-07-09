using System;

namespace Core
{
    public class SourceColumnAttribute : Attribute
    {
        public SourceColumnAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }

        public string FieldName { get; }
    }
}
