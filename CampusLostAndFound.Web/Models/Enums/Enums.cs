namespace CampusLostAndFound.Web.Models.Enums;

public enum ItemType
{
    Lost = 0,
    Found = 1
}

public enum ItemStatus
{
    Open = 0,
    Active = 0,  // Alias for Open
    Claimed = 1,
    Returned = 2,
    Resolved = 2,  // Alias for Returned
    Expired = 3,
    Cancelled = 4
}

public enum ClaimStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    ClaimUpdate = 3,
    ItemMatch = 4
}
