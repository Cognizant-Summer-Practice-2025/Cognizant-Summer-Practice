namespace backend_messages.Models
{
    public enum ConversationType
    {
        DirectMessage,
        GroupChat,
        Channel,
        Support
    }

    public enum ParticipantRole
    {
        Member,
        Admin,
        Owner,
        Moderator
    }

    public enum MessageType
    {
        Text,
        Image,
        File,
        Audio,
        Video,
        System,
        Sticker,
        Gif
    }
} 