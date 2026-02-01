import { fileURLToPath, URL } from 'node:url'
import process from 'node:process';

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vuetify from 'vite-plugin-vuetify';
import vueDevTools from 'vite-plugin-vue-devtools'

const proxyTarget = process.env.ASPNETCORE_HTTPS_PORT
  ? `https://localhost:${process.env.ASPNETCORE_HTTPS_PORT}`
  : process.env.ASPNETCORE_URLS
  ? process.env.ASPNETCORE_URLS.split(';')[0]
  : 'https://localhost:5001';


export default defineConfig({
  css: {
    preprocessorOptions: {
      scss: {
        api: 'modern-compiler',
      },
      sass: {
        api: 'modern-compiler',
      },
    }
  },
  plugins: [
    vue(),
    vueDevTools(),
    vuetify()
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  server: {
    port: 3001,
    proxy: {
      '^/api': {
        target: proxyTarget,
        secure: false,
      },
      '^/starterapp': {
        target: proxyTarget,
        secure: false,
      },
      '^/swagger': {
        target: proxyTarget,
        secure: false,
      },
      '^/Authentication': {
        target: proxyTarget,
        secure: false,
        changeOrigin: true,
      },
    },
  },
  build: {
    outDir: '../../wwwroot',
    emptyOutDir: true,
  },
});
