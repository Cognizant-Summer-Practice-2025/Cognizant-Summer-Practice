import { NextRequest, NextResponse } from 'next/server';

/**
 * Returns a tiny HTML page that sequentially redirects the browser to
 * each service URL with ?signout=1 to let services clear local storage,
 * then finally redirects to the provided return URL.
 *
 * Query params:
 *  - services: comma-separated list of absolute service origins (e.g., https://home...,https://messages...)
 *  - return: final URL to navigate back to
 */
export async function GET(request: NextRequest) {
  const { searchParams } = new URL(request.url);
  const servicesParam = searchParams.get('services') || '';
  const returnUrl = searchParams.get('return') || '/';

  // Sanitize and filter
  const services = servicesParam
    .split(',')
    .map(s => s.trim())
    .filter(s => s.startsWith('http://') || s.startsWith('https://'));

  // Redirect to client page that shows the app's Loader and performs the cascade
  const html = `<!doctype html>
<html><head><meta charset="utf-8"><title>Signing out...</title></head>
<body>
<script>
(function() {
  var services = ${JSON.stringify(services)};
  var finalUrlRaw = ${JSON.stringify(returnUrl)};
  try {
    var p = new URL('/signing-out', window.location.origin);
    p.searchParams.set('services', services.join(','));
    p.searchParams.set('return', finalUrlRaw);
    window.location.replace(p.toString());
  } catch {
    window.location.replace('/signing-out');
  }
})();
</script>
</body></html>`;

  return new NextResponse(html, {
    status: 200,
    headers: {
      'Content-Type': 'text/html; charset=utf-8',
    },
  });
}


