-- ===============================================
-- Messages Service Database Initialization
-- ===============================================

-- Enum values stored as integers for Entity Framework compatibility
-- message_type: 0=Text, 1=Image, 2=File, 3=Audio, 4=Video, 5=System

-- ===============================================
-- TABLE CREATION
-- ===============================================

-- Create conversations table (without FK to messages initially)
CREATE TABLE conversations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    initiator_id UUID NOT NULL,
    receiver_id UUID NOT NULL,
    last_message_timestamp TIMESTAMP NOT NULL DEFAULT NOW(),
    last_message_id UUID, -- FK constraint added later
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

-- Create messages table (with FK to conversations)
CREATE TABLE messages (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
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

-- ===============================================
-- INDEXES FOR PERFORMANCE
-- ===============================================

-- Messages table indexes
CREATE INDEX idx_messages_conversation_id ON messages(conversation_id);
CREATE INDEX idx_messages_sender_id ON messages(sender_id);
CREATE INDEX idx_messages_receiver_id ON messages(receiver_id);
CREATE INDEX idx_messages_created_at ON messages(created_at);
CREATE INDEX idx_messages_is_read ON messages(is_read);
CREATE INDEX idx_messages_conversation_timeline ON messages(conversation_id, created_at DESC);

-- Conversations table indexes
CREATE INDEX idx_conversations_initiator_id ON conversations(initiator_id);
CREATE INDEX idx_conversations_receiver_id ON conversations(receiver_id);
CREATE INDEX idx_conversations_last_message_id ON conversations(last_message_id);
CREATE INDEX idx_conversations_last_message_timestamp ON conversations(last_message_timestamp DESC);
CREATE INDEX idx_conversations_users ON conversations(initiator_id, receiver_id);

-- ===============================================
-- FOREIGN KEY CONSTRAINTS
-- ===============================================

-- Add FK constraint after both tables exist (prevents circular dependency)
ALTER TABLE conversations 
ADD CONSTRAINT fk_conversations_last_message 
FOREIGN KEY (last_message_id) REFERENCES messages(id) ON DELETE SET NULL;

-- ===============================================
-- DATABASE FUNCTIONS
-- ===============================================

-- Get or create conversation between two users
CREATE OR REPLACE FUNCTION create_or_get_conversation(
    sender_id UUID,
    receiver_id UUID
) RETURNS UUID AS $$
DECLARE
    conversation_id UUID;
BEGIN
    -- Find existing conversation in either direction
    SELECT id INTO conversation_id
    FROM conversations
    WHERE (initiator_id = sender_id AND receiver_id = receiver_id)
       OR (initiator_id = receiver_id AND receiver_id = sender_id)
    LIMIT 1;

    -- Return existing conversation
    IF conversation_id IS NOT NULL THEN
        RETURN conversation_id;
    END IF;

    -- Create new conversation with sender as initiator
    INSERT INTO conversations (initiator_id, receiver_id)
    VALUES (sender_id, receiver_id)
    RETURNING id INTO conversation_id;

    RETURN conversation_id;
END;
$$ LANGUAGE plpgsql;

-- Update conversation's last message reference
CREATE OR REPLACE FUNCTION update_conversation_last_message(
    conversation_id UUID,
    message_id UUID
) RETURNS VOID AS $$
BEGIN
    UPDATE conversations
    SET last_message_id = message_id,
        last_message_timestamp = NOW(),
        updated_at = NOW()
    WHERE id = conversation_id;
END;
$$ LANGUAGE plpgsql;

-- Send a message (creates conversation if needed)
CREATE OR REPLACE FUNCTION send_message(
    sender_id UUID,
    receiver_id UUID,
    message_content TEXT,
    message_type INTEGER DEFAULT 0,
    reply_to_id UUID DEFAULT NULL
) RETURNS TABLE (
    message_id UUID,
    conversation_id UUID
) AS $$
DECLARE
    conv_id UUID;
    msg_id UUID;
BEGIN
    -- Get or create conversation
    SELECT create_or_get_conversation(sender_id, receiver_id) INTO conv_id;

    -- Insert message
    INSERT INTO messages (conversation_id, sender_id, receiver_id, content, message_type, reply_to_message_id)
    VALUES (conv_id, sender_id, receiver_id, message_content, message_type, reply_to_id)
    RETURNING id INTO msg_id;

    -- Update conversation's last message
    PERFORM update_conversation_last_message(conv_id, msg_id);

    RETURN QUERY SELECT msg_id, conv_id;
END;
$$ LANGUAGE plpgsql;

-- Get user's conversations with last message info
CREATE OR REPLACE FUNCTION get_user_conversations(
    user_id UUID
) RETURNS TABLE (
    id UUID,
    other_user_id UUID,
    is_initiator BOOLEAN,
    last_message_timestamp TIMESTAMP,
    last_message_content TEXT,
    last_message_type INTEGER,
    created_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.id,
           CASE WHEN c.initiator_id = user_id THEN c.receiver_id ELSE c.initiator_id END,
           c.initiator_id = user_id,
           c.last_message_timestamp,
           m.content,
           m.message_type,
           c.created_at
    FROM conversations c
    LEFT JOIN messages m ON c.last_message_id = m.id
    WHERE c.initiator_id = user_id OR c.receiver_id = user_id
    ORDER BY c.last_message_timestamp DESC;
END;
$$ LANGUAGE plpgsql;

-- Get messages in a conversation (with pagination)
CREATE OR REPLACE FUNCTION get_conversation_messages(
    conversation_id UUID,
    page_size INTEGER DEFAULT 50,
    offset_value INTEGER DEFAULT 0
) RETURNS TABLE (
    id UUID,
    sender_id UUID,
    receiver_id UUID,
    reply_to_message_id UUID,
    content TEXT,
    message_type INTEGER,
    is_read BOOLEAN,
    created_at TIMESTAMP,
    updated_at TIMESTAMP
) AS $$
BEGIN
    RETURN QUERY
    SELECT m.id, m.sender_id, m.receiver_id, m.reply_to_message_id,
           m.content, m.message_type, m.is_read, m.created_at, m.updated_at
    FROM messages m
    WHERE m.conversation_id = get_conversation_messages.conversation_id
      AND m.deleted_at IS NULL
    ORDER BY m.created_at DESC
    LIMIT page_size OFFSET offset_value;
END;
$$ LANGUAGE plpgsql;

-- ===============================================
-- TRIGGERS FOR AUTOMATIC UPDATES
-- ===============================================

-- Trigger function: Update conversation when message is inserted
CREATE OR REPLACE FUNCTION trigger_update_conversation_last_message()
RETURNS TRIGGER AS $$
BEGIN
    UPDATE conversations
    SET last_message_id = NEW.id,
        last_message_timestamp = NEW.created_at,
        updated_at = NOW()
    WHERE id = NEW.conversation_id;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for message inserts
CREATE TRIGGER update_conversation_on_message_insert
    AFTER INSERT ON messages
    FOR EACH ROW
    EXECUTE FUNCTION trigger_update_conversation_last_message();

-- Trigger function: Handle message deletions
CREATE OR REPLACE FUNCTION trigger_handle_message_deletion()
RETURNS TRIGGER AS $$
DECLARE
    latest_message_id UUID;
    latest_message_timestamp TIMESTAMP;
BEGIN
    -- Check if deleted message was the last message
    IF OLD.id = (SELECT last_message_id FROM conversations WHERE id = OLD.conversation_id) THEN
        -- Find new last message
        SELECT id, created_at
        INTO latest_message_id, latest_message_timestamp
        FROM messages
        WHERE conversation_id = OLD.conversation_id
          AND deleted_at IS NULL
          AND id != OLD.id
        ORDER BY created_at DESC
        LIMIT 1;
        
        -- Update conversation
        UPDATE conversations
        SET last_message_id = latest_message_id,
            last_message_timestamp = COALESCE(latest_message_timestamp, last_message_timestamp),
            updated_at = NOW()
        WHERE id = OLD.conversation_id;
    END IF;
    
    RETURN OLD;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for message soft deletes
CREATE TRIGGER handle_message_deletion
    AFTER UPDATE OF deleted_at ON messages
    FOR EACH ROW
    WHEN (OLD.deleted_at IS NULL AND NEW.deleted_at IS NOT NULL)
    EXECUTE FUNCTION trigger_handle_message_deletion();

-- ===============================================
-- INITIALIZATION COMPLETE
-- ===============================================

\echo 'Messages service database initialized successfully!'
\echo 'Created tables: conversations, messages'
\echo 'Created indexes for optimal performance'
\echo 'Created functions: create_or_get_conversation, send_message, get_user_conversations, get_conversation_messages'
\echo 'Created triggers for automatic conversation updates' 