import CryptoJS from 'crypto-js';

export class MessageEncryption {
  private static readonly ENCRYPTION_PREFIX = 'ENC:';
  
  /**
   * Derive encryption key from user ID using PBKDF2
   */
  private static deriveKey(userId: string): string {
    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for key derivation');
    }

    const applicationSalt = 'portfolio-messages-app-2024';
    const combinedSalt = CryptoJS.SHA256(applicationSalt + userId).toString();
    
    // Use PBKDF2 to derive a key and convert to string for AES
    const key = CryptoJS.PBKDF2(userId, combinedSalt, {
      keySize: 256 / 32, // 256 bits = 8 words of 32 bits each
      iterations: 10000,
      hasher: CryptoJS.algo.SHA256,
    });
    
    return key.toString();
  }

  /**
   * Check if a message is encrypted (has our encryption prefix)
   */
  static isEncrypted(message: string): boolean {
    return message.startsWith(this.ENCRYPTION_PREFIX);
  }

  /**
   * Encrypt a message using user-specific key
   */
  static encrypt(message: string, userId: string): string {
    if (!message || message.trim() === '') {
      return message;
    }

    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for encryption');
    }

    // Don't double-encrypt
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

  /**
   * Decrypt a message using user-specific key
   */
  static decrypt(encryptedMessage: string, userId: string): string {
    if (!encryptedMessage || encryptedMessage.trim() === '') {
      return encryptedMessage;
    }

    // If message is not encrypted (legacy message), return as-is
    if (!this.isEncrypted(encryptedMessage)) {
      return encryptedMessage;
    }

    if (!userId || userId.trim() === '') {
      throw new Error('User ID is required for decryption');
    }

    try {
      // Remove the encryption prefix
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

  /**
   * Test function to demonstrate encryption/decryption workflow
   * This can be removed in production
   */
  static testEncryptionFlow(userId: string): void {
    const testMessage = "Hello, this is a test message!";
    console.log('🔒 Testing E2E Encryption Flow');
    console.log('Original message:', testMessage);
    
    try {
      const encrypted = this.encrypt(testMessage, userId);
      console.log('Encrypted message:', encrypted);
      console.log('Is encrypted?', this.isEncrypted(encrypted));
      
      const decrypted = this.decrypt(encrypted, userId);
      console.log('Decrypted message:', decrypted);
      
      const isValid = testMessage === decrypted;
      console.log('✅ Encryption test:', isValid ? 'PASSED' : 'FAILED');
      
      if (!isValid) {
        console.error('❌ Original and decrypted messages do not match!');
      }

      // Test legacy message handling
      const legacyMessage = "This is a legacy unencrypted message";
      const legacyDecrypted = this.decrypt(legacyMessage, userId);
      console.log('🔄 Legacy message handling:', legacyMessage === legacyDecrypted ? 'PASSED' : 'FAILED');
      
    } catch (error) {
      console.error('❌ Encryption test failed:', error);
    }
  }
} 