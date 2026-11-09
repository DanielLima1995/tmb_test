import { defineConfig, loadEnv } from "vite";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    server: {
      proxy: {
        "/api": {
          target: env.VITE_API_PROXY_TARGET || "http://localhost:5161",
          changeOrigin: true,
        },
        "/hubs": {
          target: env.VITE_API_PROXY_TARGET || "http://localhost:5161",
          changeOrigin: true,
          ws: true,
        },
      },
    },
  };
});
