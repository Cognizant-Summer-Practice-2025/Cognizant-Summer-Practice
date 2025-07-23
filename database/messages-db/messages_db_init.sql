-- Enum values stored as integers for Entity Framework compatibility
-- message_type: 0=Text, 1=Image, 2=File, 3=Audio, 4=Video, 5=System

-- Messages table
CREATE TABLE messages (
                          id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                          sender_id UUID NOT NULL,
                          receiver_id UUID NOT NULL,
                          reply_to_message_id UUID REFERENCES messages(id) ON DELETE SET NULL,
                          content TEXT,
                          message_type INTEGER NOT NULL DEFAULT 0,
                          is_read BOOLEAN NOT NULL DEFAULT FALSE,
                          created_at TIMESTAMP NOT NULL DEFAULT NOW(),
                          updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
                          deleted_at TIMESTAMP
);

-- Create indexes for performance
CREATE INDEX idx_messages_sender_id ON messages(sender_id);
CREATE INDEX idx_messages_receiver_id ON messages(receiver_id);
CREATE INDEX idx_messages_created_at ON messages(created_at);
CREATE INDEX idx_messages_is_read ON messages(is_read);
-- Composite index for conversation queries (both directions)
CREATE INDEX idx_messages_conversation ON messages(sender_id, receiver_id, created_at DESC);
CREATE INDEX idx_messages_conversation_reverse ON messages(receiver_id, sender_id, created_at DESC);

-- Function to get the last message between two users (conversation)
CREATE OR REPLACE FUNCTION get_last_message_between_users(
    user1_id UUID,
    user2_id UUID
) RETURNS TABLE (
    id UUID,
    sender_id UUID,
    receiver_id UUID,
    reply_to_message_id UUID,
    content TEXT,
    message_type INTEGER,
    is_read BOOLEAN,
    created_at TIMESTAMP,
    updated_at TIMESTAMP,
    deleted_at TIMESTAMP
) AS $$
BEGIN
RETURN QUERY
SELECT m.id, m.sender_id, m.receiver_id, m.reply_to_message_id,
       m.content, m.message_type, m.is_read, m.created_at, m.updated_at, m.deleted_at
FROM messages m
WHERE (
    (m.sender_id = user1_id AND m.receiver_id = user2_id) OR
    (m.sender_id = user2_id AND m.receiver_id = user1_id)
    )
  AND m.deleted_at IS NULL
ORDER BY m.created_at DESC
    LIMIT 1;
END;
$$ LANGUAGE plpgsql;

-- Success message
\echo 'Messages service database initialized successfully!'
\echo 'Created tables: messages' 