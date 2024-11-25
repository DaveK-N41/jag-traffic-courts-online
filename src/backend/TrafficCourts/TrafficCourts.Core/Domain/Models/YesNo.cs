namespace TrafficCourts.Domain.Models;

public enum YesNo
{

    [System.Runtime.Serialization.EnumMember(Value = @"UNKNOWN")]
    Unknown = 0,

    [System.Runtime.Serialization.EnumMember(Value = @"Y")]
    Yes = 1,

    [System.Runtime.Serialization.EnumMember(Value = @"N")]
    No = 2,

}
