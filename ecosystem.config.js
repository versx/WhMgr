module.exports = {
  apps: [{
    name: "WhMgr",
    script: "WhMgr.dll",
    args: "--config config.json --name test",
    watch: true,
    cwd: "/home/user/WhMgr/bin",
    interpreter: "dotnet",
    instances: 1,
    exec_mode: "fork"
  }]
};
