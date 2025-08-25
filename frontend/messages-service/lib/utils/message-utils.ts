export const formatTimestamp = (dateString: string): string => {
  if (!dateString || dateString.trim() === '') {
    return 'Now';
  }
  
  const date = new Date(dateString);
  
  if (isNaN(date.getTime())) {
    return 'Now';
  }
  
  const now = new Date();
  const diffInHours = Math.floor((now.getTime() - date.getTime()) / (1000 * 60 * 60));
  
  if (diffInHours < 1) {
    const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60));
    return `${diffInMinutes}m`;
  } else if (diffInHours < 24) {
    return `${diffInHours}h`;
  } else {
    const diffInDays = Math.floor(diffInHours / 24);
    return `${diffInDays}d`;
  }
};

export const formatMessageTimestamp = (dateString: string): string => {
  if (!dateString || dateString.trim() === '') {
    dateString = new Date().toISOString();
  }
  
  const utcDate = new Date(dateString + (dateString.endsWith('Z') ? '' : 'Z'));
  
  if (isNaN(utcDate.getTime())) {
    return formatMessageTimestamp(new Date().toISOString());
  }
  
  const now = new Date();
  const diffInHours = (now.getTime() - utcDate.getTime()) / (1000 * 60 * 60);
  
  if (diffInHours < 12) {
    return utcDate.toLocaleTimeString(undefined, { 
      hour: 'numeric', 
      minute: '2-digit',
      hour12: true
    });
  } else {
    return utcDate.toLocaleDateString(undefined, {
      month: 'short',
      day: 'numeric',
      year: utcDate.getFullYear() !== now.getFullYear() ? 'numeric' : undefined
    });
  }
};

export const getValidTimestamp = (...timestamps: (string | undefined)[]): string => {
  for (const timestamp of timestamps) {
    if (timestamp && timestamp.trim() !== '') {
      const testDate = new Date(timestamp);
      if (!isNaN(testDate.getTime())) {
        return timestamp;
      }
    }
  }
  return new Date().toISOString();
};

export const truncateMessage = (message: string, maxLength: number = 100): string => {
  if (message && message.length > maxLength) {
    return `${message.substring(0, maxLength)}...`;
  }
  return message;
}; 