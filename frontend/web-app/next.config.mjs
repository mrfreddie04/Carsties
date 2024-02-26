/** @type {import('next').NextConfig} */
const nextConfig = {
  // experimental: {
  //   serverActions: true
  // },
  logging: {
    fetches: {
      fullUrl: true,
    },
  },
  images: {
    remotePatterns: [
      {
        protocol: 'https',
        hostname: 'cdn.pixabay.com',
        pathname: '**',
        port: ''
      },
    ],    
    // domains: ["cdn.pixabay.com"]
  },
  //output: 'export'
  output: "standalone",
};

export default nextConfig;
