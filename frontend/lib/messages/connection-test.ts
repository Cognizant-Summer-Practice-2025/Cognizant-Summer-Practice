// Connection test utilities for the Messages API

const API_BASE_URL = 'http://localhost:5093';

export interface ConnectionTestResult {
  success: boolean;
  message: string;
  timestamp?: string;
  error?: string;
}

/**
 * Test basic API connection
 */
export async function testApiConnection(): Promise<ConnectionTestResult> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Test/ping`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        success: false,
        message: 'API connection failed',
        error: `HTTP ${response.status}: ${response.statusText}`
      };
    }

    const data = await response.json();
    return {
      success: true,
      message: data.message || 'API connection successful',
      timestamp: data.timestamp
    };
  } catch (error) {
    return {
      success: false,
      message: 'Cannot reach Messages API',
      error: error instanceof Error ? error.message : 'Unknown error'
    };
  }
}

/**
 * Test CORS configuration
 */
export async function testCorsConnection(): Promise<ConnectionTestResult> {
  try {
    const response = await fetch(`${API_BASE_URL}/api/Test/cors`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        success: false,
        message: 'CORS test failed',
        error: `HTTP ${response.status}: ${response.statusText}`
      };
    }

    const data = await response.json();
    return {
      success: true,
      message: data.message || 'CORS configuration working',
      timestamp: data.timestamp
    };
  } catch (error) {
    return {
      success: false,
      message: 'CORS configuration issue',
      error: error instanceof Error ? error.message : 'Unknown error'
    };
  }
}

/**
 * Test full message API functionality
 */
export async function testMessageApi(): Promise<ConnectionTestResult> {
  try {
    // Test user conversations endpoint
    const userId = "11111111-1111-1111-1111-111111111111"; // Test user ID
    const response = await fetch(`${API_BASE_URL}/api/Conversation/user/${userId}`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
    });

    if (!response.ok) {
      return {
        success: false,
        message: 'Message API endpoints not working',
        error: `HTTP ${response.status}: ${response.statusText}`
      };
    }

    const conversations = await response.json();
    return {
      success: true,
      message: `Message API working - Found ${conversations.length} conversations`,
      timestamp: new Date().toISOString()
    };
  } catch (error) {
    return {
      success: false,
      message: 'Message API test failed',
      error: error instanceof Error ? error.message : 'Unknown error'
    };
  }
}

/**
 * Run all connection tests
 */
export async function runAllConnectionTests(): Promise<{
  api: ConnectionTestResult;
  cors: ConnectionTestResult;
  messageApi: ConnectionTestResult;
  overall: boolean;
}> {
  console.log('🔄 Running connection tests...');
  
  const api = await testApiConnection();
  console.log('📡 API Test:', api);
  
  const cors = await testCorsConnection();
  console.log('🌐 CORS Test:', cors);
  
  const messageApi = await testMessageApi();
  console.log('💬 Message API Test:', messageApi);
  
  const overall = api.success && cors.success && messageApi.success;
  
  console.log(overall ? '✅ All tests passed!' : '❌ Some tests failed');
  
  return { api, cors, messageApi, overall };
} 