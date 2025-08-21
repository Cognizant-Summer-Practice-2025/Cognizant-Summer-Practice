export async function GET() {
  return Response.json({
    status: 'ok',
    service: 'admin-service',
    timestamp: new Date().toISOString(),
  });
}


