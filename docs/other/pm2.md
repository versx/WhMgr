# PM2 Configuration

**Install PM2**  
https://pm2.keymetrics.io/docs/usage/quick-start/#installation

**Create PM2 Config**  
Create `ecosystem.config.js` file with below example file. (can be named anything)  

**Run**  
Start with `pm2 start ecosystem.config.js`  


Example PM2 ecosystem configuration file:  
```js
module.exports = {
  apps: [
    {
      name: "WhMgr1",
      script: "WhMgr.dll",
	  args: "--name Test --config config.test.json",
	  watch: true,
	  cwd: "/home/user/whmgr/bin",
	  interpreter: "dotnet",
      max_memory_restart: "2G",
      autorestart: true,
      instances: 1,
      exec_mode: "fork"
    },
    {
        ...
    }
  ]
};
```

**PM2 Auto Startup Instructions**  
https://pm2.keymetrics.io/docs/usage/startup/