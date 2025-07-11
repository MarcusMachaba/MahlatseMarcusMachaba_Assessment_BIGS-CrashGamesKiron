using DatabaseLayer.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI
{
    [TableContract(PrimaryKey = nameof(Logs.Id))]
    public class Logs : HasIdOnly
    {
        [DataMember, ColumnContract(Length = 4000)] public string Message { get; set; }
        [DataMember, ColumnContract(Length = 4000)] public string message_template { get; set; }
        [DataMember, ColumnContract(Length = 50)] public string Level { get; set; }
        [DataMember, ColumnContract] public DateTime time_stamp { get; set; }
        [DataMember, ColumnContract] public DateTime Date { get; set; }
        [DataMember, ColumnContract(Length = 4000)] public string Exception { get; set; }
        [DataMember, ColumnContract(Length = 255)] public string Logger { get; set; }
        [DataMember, ColumnContract(Length = 4000)] public string log_event { get; set; }
        [DataMember, ColumnContract(Length = 255)] public string MachineName { get; set; }
        [DataMember, ColumnContract(Length = 4000)] public string application { get; set; }
        [DataMember, ColumnContract] public long user_id { get; set; }
        [DataMember, ColumnContract(Length = 4000)] public string user_name { get; set; }
        [DataMember, ColumnContract(Length = 255)] public string Thread { get; set; }
    }
}
