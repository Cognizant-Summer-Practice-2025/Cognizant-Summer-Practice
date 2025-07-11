namespace backend_user.Models
{
    public enum Provider
    {
        Google,
        GitHub,
        LinkedIn,
        Microsoft,
        Apple
    }

    public enum ProfileVisibility
    {
        Public,
        Private,
        ContactsOnly
    }

    public enum Theme
    {
        Light,
        Dark,
        System
    }

    public enum TargetType
    {
        User,
        Portfolio,
        Message,
        BlogPost,
        Project
    }

    public enum ActionType
    {
        Create,
        Update,
        Delete,
        Suspend,
        Activate,
        Feature,
        Unfeature,
        Ban,
        Unban
    }

    public enum ReportType
    {
        Spam,
        Harassment,
        InappropriateContent,
        Copyright,
        Impersonation,
        FakeProfile,
        Other
    }

    public enum ReportStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Dismissed,
        Escalated
    }
} 