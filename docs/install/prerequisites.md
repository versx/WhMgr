# Prerequisites  
- Git  
- .NET 5 SDK  


#### __Git__  
Install [Git](https://git-scm.com/book/en/v2/Getting-Started-Installing-Git)  

#### __.NET 5 SDK__  
```
# Download install script
wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh

# Make installer executable
chmod +x dotnet-install.sh

# Install .NET 5.0 SDK
./dotnet-install.sh --version 5.0.404
```

<hr>

### __Automated Install Scripts__  
Run the following to install .NET 5 software development kit (SDK), clone respository, and copy default example embeds, filters, geofences, config and alarm files.  

__Linux/macOS__  
```
wget https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/scripts/install.sh && chmod +x install.sh && ./install.sh && rm install.sh  
```
__Windows__  
```
bitsadmin /transfer dotnet-install-job /download /priority FOREGROUND https://raw.githubusercontent.com/versx/WhMgr/v5-rewrite/scripts/install.bat install.bat | start install.bat  
```