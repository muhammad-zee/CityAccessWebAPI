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
        Pending = 0,
        Reserved = 1,
        Completed = 2,
        Cancelled = 3
    }

    public enum ReservationStatusEnums
    {
        Ringing = 0,
        Connected = 1,
        Completed = 2,
        Cancelled = 3,
        NoAnswer = 4,
        Busy = 5,
    }

}
