// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: "2025-07-15",
  devtools: { enabled: true },

  vite: {
    server: {
      proxy: {
        "/graphql": {
          target: "http://localhost:5095/graphql",
          changeOrigin: true,
        },
      },
    },
  },

  modules: ["@pinia/nuxt"],
});
