// Fetches every published post from the live API and writes one route per
// line to routes.txt, consumed by `ng run blog-frontend:prerender --routes-file`.
// Static routes (the ones with no dynamic id) are listed alongside.
const https = require('https');
const fs = require('fs');
const path = require('path');

const API_URL = 'https://blogapi-backend-ncul.onrender.com/api/posts?pageNumber=1&pageSize=5000';
const OUTPUT_FILE = path.join(__dirname, '..', 'routes.txt');
const STATIC_ROUTES = ['/posts', '/privacy'];

https.get(API_URL, res => {
  let body = '';
  res.on('data', chunk => (body += chunk));
  res.on('end', () => {
    let postRoutes = [];
    try {
      const json = JSON.parse(body);
      const items = json?.data?.items ?? [];
      postRoutes = items.map(post => `/posts/${post.id}`);
    } catch (err) {
      console.error('Failed to parse posts response, falling back to static routes only:', err.message);
    }

    const routes = [...STATIC_ROUTES, ...postRoutes];
    fs.writeFileSync(OUTPUT_FILE, routes.join('\n') + '\n');
    console.log(`Wrote ${routes.length} routes (${postRoutes.length} posts) to ${OUTPUT_FILE}`);
  });
}).on('error', err => {
  console.error('Failed to fetch posts, falling back to static routes only:', err.message);
  fs.writeFileSync(OUTPUT_FILE, STATIC_ROUTES.join('\n') + '\n');
});
