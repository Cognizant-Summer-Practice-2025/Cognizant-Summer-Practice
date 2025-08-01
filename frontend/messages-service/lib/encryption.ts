import CryptoJS from 'crypto-js';

export class MessageEncryption {
  private static readonly ENCRYPTION_PREFIX = 'ENC:';

  private static deriveKey(userId: string): string {
    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for key derivation');
    }

    const applicationSalt = 'portfolio-messages-app-2024';
    const combinedSalt = CryptoJS.SHA256(applicationSalt + userId).toString();
    
    const key = CryptoJS.PBKDF2(userId, combinedSalt, {
      keySize: 256 / 32,
      iterations: 10000,
      hasher: CryptoJS.algo.SHA256,
    });
    
    return key.toString();
  }

  static isEncrypted(message: string): boolean {
    return message.startsWith(this.ENCRYPTION_PREFIX);
  }

  static encrypt(message: string, userId: string): string {
    if (!message || message.trim() === '') {
      return message;
    }

    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for encryption');
    }

    if (this.isEncrypted(message)) {
      return message;
    }

    try {
      const key = this.deriveKey(userId);
      const encrypted = CryptoJS.AES.encrypt(message, key).toString();
      return this.ENCRYPTION_PREFIX + encrypted;
    } catch (error) {
      console.error('Failed to encrypt message:', error);
      throw error;
    }
  }

  static decrypt(encryptedMessage: string, userId: string): string {
    if (!encryptedMessage || encryptedMessage.trim() === '') {
      return encryptedMessage;
    }

    if (!this.isEncrypted(encryptedMessage)) {
      return encryptedMessage;
    }

    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for decryption');
    }

    try {
      const actualEncryptedData = encryptedMessage.substring(this.ENCRYPTION_PREFIX.length);
      
      const key = this.deriveKey(userId);
      const decrypted = CryptoJS.AES.decrypt(actualEncryptedData, key);
      const decryptedText = decrypted.toString(CryptoJS.enc.Utf8);
      
      if (!decryptedText) {
        throw new Error('Failed to decrypt message - invalid format or wrong key');
      }
      
      return decryptedText;
    } catch (error) {
      console.error('Failed to decrypt message:', error);
      throw error;
    }
  }
} 