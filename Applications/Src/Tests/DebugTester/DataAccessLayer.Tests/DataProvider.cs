using DatabaseLayer;
using DatabaseLayer.Interfaces;
using DatabaseLayer.Metadata;
using DatabaseLayer.SqlServerProvider.DataObjectInterfaces;
using DebugTester.DataAccessLayer.Tests.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugTester.DataAccessLayer.Tests
{
    public class DataProvider : BaseDataProvider
    {
        //this connString wont be hard coded like this but will be from configFile (just hardCoding for Demo Purposed)
        public override string ConnectionString => "Data Source=.;Initial Catalog=DataAccesLayer_DemoDB_test1;integrated security=true;TrustServerCertificate=true";
        public IDataObjectInterface<Logs> Logs { get; set; }
        public IDataObjectInterface<Bank> Banks { get; set; }
        public IDataObjectInterface<Branch> Branches { get; set; }
        public IDataObjectInterface<Logs2> Logs2 { get; set; }
        public DataProvider()
        {
            Logs = new SpDataObjectInterface<Logs>(this);
            Logs2 = new SpDataObjectInterface<Logs2>(this);
            Banks = new SpDataObjectInterface<Bank>(this);
            Branches = new SpDataObjectInterface<Branch>(this);
        }

        public override void ConfigureDefaultData()
        {
            //throw new NotImplementedException();
            // This is empty because this is a default implementation that will be used when this extension is not used by the developer
        }

        public override string GetTableName(Type type)
        {
            return $"{type.Name}";
        }

        protected override DbIndex[] GetIndices()
        {
            return new DbIndex[0];
        }
    }
}
