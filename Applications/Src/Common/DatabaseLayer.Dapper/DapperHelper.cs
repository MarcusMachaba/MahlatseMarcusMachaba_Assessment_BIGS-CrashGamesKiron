using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseLayer.Dapper
{
    public class DapperHelper
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();
        private readonly SortedDictionary<string, object> _conditionalParameters = new SortedDictionary<string, object>();

        private readonly string _tableName;
        private readonly string _whereSql;

        public DapperHelper(string tableName, string whereSql = null)
        {
            _tableName = tableName;
            _whereSql = whereSql;
        }


        /// <summary>
        /// Adds the supplied parameterName to value mapping to a collection
        /// of items used in the insert and update values to be modified.
        /// This method does not check for existing values
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="value"></param>
        public void Add(string parameterName, object value)
        {
            if (_parameters.ContainsKey(parameterName))
                throw new DuplicateNameException("This field was already declared");

            _parameters.Add(parameterName, value);
        }



        /// <summary>
        /// Creates an insert statement for a single record that returns the bigint 
        /// identity of the record inserted
        /// Throws an exception if the collection is empty
        /// </summary>
        public string InsertSql
        {
            get
            {
                if (_parameters.Keys.Count == 0)
                    throw new Exception("Attempted to perform an insert without any input parameters.");

                var fields = string.Join(", ", _parameters.Keys);
                var values = string.Join(", @", _parameters.Keys);
                return "DECLARE @output table(_Id bigint); " +
                        $"INSERT INTO {_tableName}({fields}) " +
                        "OUTPUT INSERTED.[_Id] " +
                        "INTO @output " +
                        $"VALUES(@{values}) " +
                        "SELECT * FROM @output;";
            }
        }


        /// <summary>
        /// Generates a DynamicParameters instance based on the key value pairs
        /// that have been added
        /// </summary>
        public DynamicParameters Parameters
        {
            get
            {
                var parms = new DynamicParameters();
                foreach (var parameterName in _parameters.Keys)
                {
                    parms.Add(parameterName, _parameters[parameterName]);
                }
                foreach (var parameterName in _conditionalParameters.Keys)
                {
                    parms.Add(parameterName, _conditionalParameters[parameterName]);
                }
                return parms;
            }
        }
    }
}
