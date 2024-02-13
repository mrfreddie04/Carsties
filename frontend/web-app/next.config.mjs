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
    domains: ["cdn.pixabay.com"]
  }
};

export default nextConfig;
