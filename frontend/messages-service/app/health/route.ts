export async function GET() {
  return Response.json({
    status: 'ok',
    service: 'messages-service',
    timestamp: new Date().toISOString(),
  });
}


