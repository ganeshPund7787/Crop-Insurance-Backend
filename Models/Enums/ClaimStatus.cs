namespace Authentication.Models.Enums;

public enum ClaimStatus
{
    Submitted,      // Farmer submitted
    UnderReview,    // Agent assigned and reviewing
    InspectionScheduled,
    InspectionDone,
    Approved,
    Rejected,
    Cancelled
}