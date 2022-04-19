namespace Web.Services.Enums
{
    public enum CallEventEnums
    {
        Rejected,
        Accepted,
        HangedUp,
    }
    public enum CallDirectionEnums
    {
        Outbound,
        Inbound
    }

    public enum QueueStatusEnums
    {
        Pending=0,
        Reserved=1,
        Completed=2,
        Cancelled=3
    } 

}
