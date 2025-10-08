module.exports = {
  apps: [
    {
      name: "mic-cpanel",
      script: "server.js",
      instances: 1, // Set to 'max' for cluster mode
      exec_mode: "fork", // Use 'cluster' for load balancing
      env: {
        NODE_ENV: "development",
        PORT: 5000,
      },
      env_production: {
        NODE_ENV: "production",
        PORT: process.env.PORT || 5000,
      },
      // Logging
      log_file: "./logs/combined.log",
      out_file: "./logs/out.log",
      error_file: "./logs/error.log",
      log_date_format: "YYYY-MM-DD HH:mm:ss Z",

      // Auto restart settings
      autorestart: true,
      watch: false, // Set to true for development
      max_memory_restart: "1G",

      // Advanced settings
      min_uptime: "10s",
      max_restarts: 10,

      // Environment variables
      env_file: ".env",

      // Graceful shutdown
      kill_timeout: 5000,
      listen_timeout: 3000,

      // Monitoring
      pmx: true,

      // Source map support
      source_map_support: true,
    },
  ],

  // Deployment configuration
  deploy: {
    production: {
      user: "node",
      host: "your-server.com",
      ref: "origin/main",
      repo: "git@github.com:your-username/mic-cpanel.git",
      path: "/var/www/mic-cpanel",
      "pre-deploy-local": "",
      "post-deploy": "npm install && pm2 reload ecosystem.config.js --env production",
      "pre-setup": "",
    },
  },
};
