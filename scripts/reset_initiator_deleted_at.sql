-- ===============================================
-- Reset initiator_deleted_at Script
-- ===============================================
-- This script sets the initiator_deleted_at column to NULL 
-- for all rows in the conversations table
-- ===============================================

-- Update all rows in conversations table to set initiator_deleted_at to NULL
UPDATE conversations 
SET initiator_deleted_at = NULL 
WHERE initiator_deleted_at IS NOT NULL;

-- Show the number of affected rows
SELECT 'Script completed. Rows where initiator_deleted_at was reset:' as message;

-- Optional: Verify the update by counting rows with NULL initiator_deleted_at
SELECT COUNT(*) as total_conversations_with_null_initiator_deleted_at
FROM conversations 
WHERE initiator_deleted_at IS NULL; 