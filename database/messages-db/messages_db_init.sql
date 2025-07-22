-- Create ENUMs for Messages Service
CREATE TYPE conversation_type AS ENUM ('DirectMessage', 'GroupChat', 'Channel');
CREATE TYPE participant_role AS ENUM ('Member', 'Admin', 'Moderator', 'Owner');
CREATE TYPE message_type AS ENUM ('Text', 'Image', 'File', 'Audio', 'Video', 'System');

-- Conversations table
CREATE TABLE conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    last_message_id UUID,
    type conversation_type NOT NULL,
    title VARCHAR(255),
    last_activity_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Messages table
CREATE TABLE messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    sender_id UUID NOT NULL,
    reply_to_message_id UUID REFERENCES messages(id) ON DELETE SET NULL,
    content TEXT,
    message_type message_type NOT NULL DEFAULT 'Text',
    attachment_url TEXT,
    attachment_filename VARCHAR(255),
    attachment_size INTEGER,
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    is_edited BOOLEAN NOT NULL DEFAULT FALSE,
    edited_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    deleted_at TIMESTAMP
);

ALTER TABLE conversations 
ADD CONSTRAINT fk_conversations_last_message 
FOREIGN KEY (last_message_id) REFERENCES messages(id) ON DELETE SET NULL;

-- Conversation Participants table
CREATE TABLE conversation_participants (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    user_id UUID NOT NULL,
    last_read_message_id UUID REFERENCES messages(id) ON DELETE SET NULL,
    joined_at TIMESTAMP NOT NULL DEFAULT NOW(),
    left_at TIMESTAMP,
    is_muted BOOLEAN NOT NULL DEFAULT FALSE,
    role participant_role NOT NULL DEFAULT 'Member',
    UNIQUE(conversation_id, user_id)
);

-- User Cache table
CREATE TABLE user_cache (
    user_id UUID PRIMARY KEY,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    avatar_url TEXT,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_updated TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create indexes for performance
CREATE INDEX idx_conversations_last_activity_at ON conversations(last_activity_at);
CREATE INDEX idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX idx_messages_sender_id ON messages(sender_id);
CREATE INDEX idx_messages_created_at ON messages(created_at);
CREATE INDEX idx_conversation_participants_conversation_id ON conversation_participants(conversation_id);
CREATE INDEX idx_conversation_participants_user_id ON conversation_participants(user_id);
CREATE INDEX idx_message_reads_message_id ON message_reads(message_id);
CREATE INDEX idx_message_reads_user_id ON message_reads(user_id);
CREATE INDEX idx_messages_is_read ON messages(is_read);

-- Success message
\echo 'Messages service database initialized successfully!'
\echo 'Created tables: conversations, messages, conversation_participants, message_reads, user_cache' 