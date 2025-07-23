import React, { useState, useEffect } from 'react';
import { Input, Modal, Avatar, Spin, Empty } from 'antd';
import { Search } from 'lucide-react';
import { searchUsers, SearchUser } from '@/lib/user';
import './style.css';

interface UserSearchModalProps {
  visible: boolean;
  onClose: () => void;
  onUserSelect: (user: SearchUser) => void;
}

const UserSearchModal: React.FC<UserSearchModalProps> = ({
  visible,
  onClose,
  onUserSelect
}) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [searchResults, setSearchResults] = useState<SearchUser[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!searchTerm.trim()) {
      setSearchResults([]);
      return;
    }

    const delayedSearch = setTimeout(async () => {
      setLoading(true);
      setError(null);
      
      try {
        const results = await searchUsers(searchTerm);
        setSearchResults(results);
      } catch (err) {
        console.error('Search error:', err);
        setError('Failed to search users. Please try again.');
        setSearchResults([]);
      } finally {
        setLoading(false);
      }
    }, 300);

    return () => clearTimeout(delayedSearch);
  }, [searchTerm]);

  const handleUserClick = (user: SearchUser) => {
    onUserSelect(user);
    onClose();
    setSearchTerm('');
    setSearchResults([]);
  };

  const handleClose = () => {
    onClose();
    setSearchTerm('');
    setSearchResults([]);
    setError(null);
  };

  return (
    <Modal
      title="Search Users"
      open={visible}
      onCancel={handleClose}
      footer={null}
      width={500}
      className="user-search-modal"
    >
      <div className="user-search-content">
        <div className="search-input-wrapper">
          <Input
            placeholder="Search by name or username..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            prefix={<Search size={16} />}
            className="search-input"
            autoFocus
          />
        </div>

        <div className="search-results">
          {loading && (
            <div className="loading-container">
              <Spin size="large" />
              <p>Searching users...</p>
            </div>
          )}

          {error && (
            <div className="error-container">
              <p className="error-message">{error}</p>
            </div>
          )}

          {!loading && !error && searchTerm && searchResults.length === 0 && (
            <Empty
              description="No users found"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          )}

          {!loading && !error && searchResults.length > 0 && (
            <div className="results-list">
              {searchResults.map((user) => (
                <div
                  key={user.id}
                  className="user-result-item"
                  onClick={() => handleUserClick(user)}
                >
                  <Avatar
                    src={user.avatarUrl}
                    size={40}
                    className="user-avatar"
                  >
                    {user.fullName.charAt(0).toUpperCase()}
                  </Avatar>
                  <div className="user-info">
                    <div className="user-name">{user.fullName}</div>
                    <div className="user-username">@{user.username}</div>
                    {user.professionalTitle && (
                      <div className="user-title">{user.professionalTitle}</div>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}

          {!searchTerm && (
            <div className="placeholder-content">
              <p>Start typing to search for users by name or username</p>
            </div>
          )}
        </div>
      </div>
    </Modal>
  );
};

export default UserSearchModal; 