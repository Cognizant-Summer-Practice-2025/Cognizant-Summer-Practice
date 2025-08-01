import React, { useState, useEffect } from 'react';
import { Search } from 'lucide-react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Avatar, AvatarImage, AvatarFallback } from '@/components/ui/avatar';
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
    <Dialog open={visible} onOpenChange={handleClose}>
      <DialogContent className="user-search-modal max-w-md">
        <DialogHeader>
          <DialogTitle>Search Users</DialogTitle>
        </DialogHeader>
        <div className="user-search-content">
        <div className="search-input-wrapper">
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search by name or username..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="search-input pl-10"
              autoFocus
            />
          </div>
        </div>

        <div className="search-results">
          {loading && (
            <div className="loading-container flex flex-col items-center justify-center py-8">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900"></div>
              <p className="mt-2 text-sm text-muted-foreground">Searching users...</p>
            </div>
          )}

          {error && (
            <div className="error-container p-4 text-center">
              <p className="error-message text-sm text-red-600">{error}</p>
            </div>
          )}

          {!loading && !error && searchTerm && searchResults.length === 0 && (
            <div className="empty-container flex flex-col items-center justify-center py-8">
              <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
                <Search className="w-8 h-8 text-gray-400" />
              </div>
              <p className="text-sm text-muted-foreground">No users found</p>
            </div>
          )}

          {!loading && !error && searchResults.length > 0 && (
            <div className="results-list">
              {searchResults.map((user) => (
                <div
                  key={user.id}
                  className="user-result-item"
                  onClick={() => handleUserClick(user)}
                >
                  <Avatar className="user-avatar w-10 h-10">
                    <AvatarImage src={user.avatarUrl} alt={user.fullName} />
                    <AvatarFallback>{user.fullName.charAt(0).toUpperCase()}</AvatarFallback>
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
            <div className="placeholder-content p-4 text-center">
              <p className="text-sm text-muted-foreground">Start typing to search for users by name or username</p>
            </div>
          )}
        </div>
      </div>
      </DialogContent>
    </Dialog>
  );
};

export default UserSearchModal; 