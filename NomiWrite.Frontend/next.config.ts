import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  devIndicators: false,
  experimental: {
    // Avoid restoring stale CSS across UI changes on the local Windows checkout.
    turbopackFileSystemCacheForDev: false,
  },
};

export default nextConfig;
