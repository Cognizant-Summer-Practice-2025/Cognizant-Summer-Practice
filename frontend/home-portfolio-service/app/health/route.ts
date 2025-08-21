export async function GET() {
  return Response.json({
    status: 'ok',
    service: 'home-portfolio-service',
    timestamp: new Date().toISOString(),
  });
}


