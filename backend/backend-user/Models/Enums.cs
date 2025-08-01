namespace backend_user.Models
{
    public enum OAuthProviderType
    {
        Google,
        GitHub,
        LinkedIn,
        Facebook
    }

    public enum ReportedType
    {
        User,
        Portfolio,
        Message,
        BlogPost,
        Comment
    }

    public enum ReportType
    {
        Spam,
        Harassment,
        InappropriateContent,
        FakeProfile,
        Copyright,
        Other
    }

    public enum ReportStatus
    {
        Pending,
        UnderReview,
        Resolved,
        Dismissed
    }
} 