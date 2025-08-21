export async function GET() {
  return Response.json({
    status: 'ok',
    service: 'auth-user-service',
    timestamp: new Date().toISOString(),
  });
}


