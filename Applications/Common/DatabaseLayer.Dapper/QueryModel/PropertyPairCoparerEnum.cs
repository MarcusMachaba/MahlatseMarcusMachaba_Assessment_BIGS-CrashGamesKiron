using Dapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseLayer.Dapper.QueryModel
{
    public enum PropertyPairCoparerEnum
    {
        Equals,
        NotEquals,
        Greater,
        GreaterEquals,
        Less,
        LessEquals
    }
}
