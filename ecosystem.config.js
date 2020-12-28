module.exports = {
  apps: [{
    name: "WhMgr",
    script: "WhMgr.dll",
    watch: true,
    cwd: "/home/user/WhMgr/bin",
    interpreter: "dotnet",
    instances: 1,
    exec_mode: "fork"
  }]
};
