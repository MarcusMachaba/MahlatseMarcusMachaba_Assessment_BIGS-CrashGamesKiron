using Core;
using DatabaseLayer.Attributes;
using System;
using System.Runtime.Serialization;

namespace DebugTester.DataAccessLayer.Tests.Models
{
    [TableContract(PrimaryKey = nameof(Logs.Id))]
    public class Logs : HasIdOnly
    {
        [DataMember, ColumnContract] public string Message { get; set; }
        [DataMember, ColumnContract] public string message_template { get; set; }
        [DataMember, ColumnContract] public string Level { get; set; }
        [DataMember, ColumnContract] public DateTime time_stamp { get; set; }
        [DataMember, ColumnContract] public DateTime Date { get; set; }
        [DataMember, ColumnContract] public string Exception { get; set; }
        [DataMember, ColumnContract] public string Logger { get; set; }
        [DataMember, ColumnContract] public string log_event { get; set; }
        [DataMember, ColumnContract] public string MachineName { get; set; }
        [DataMember, ColumnContract] public string application { get; set; }
        [DataMember, ColumnContract] public long user_id { get; set; }
        [DataMember, ColumnContract] public string user_name { get; set; }
        [DataMember, ColumnContract] public string Thread { get; set; }
    }
}
