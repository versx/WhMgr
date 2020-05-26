# Download .NET Core 2.1 installer
echo "Downloading .NET Core 2.1 installer..."
wget https://dotnetwebsite.azurewebsites.net/download/dotnet-core/scripts/v1/dotnet-install.sh

# Make installer executable
echo "Setting executable permissions..."
chmod +x dotnet-install.sh

# Install .NET Core 2.1.0
echo "Launching .NET Core installer..."
./dotnet-install.sh --version 2.1.803

# Delete .NET Core 2.1.0 installer
echo "Deleting .NET Core installer..."
rm dotnet-install.sh

# Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr -b netcore

# Change directory into cloned repository
echo "Changing directory..."
cd WhMgr

# Build WhMgr.dll
echo "Building WhMgr..."
~/.dotnet/dotnet build

# Copy example config
echo "Copying example files..."
cp config.example.json bin/Debug/netcoreapp2.1/config.json
cp -R examples/Alerts bin/Debug/netcoreapp2.1/Alerts/
cp -R examples/Filters bin/Debug/netcoreapp2.1/Filters/
cp -R examples/Geofences bin/Debug/netcoreapp2.1/Geofences/
cp -R static/ bin/Debug/netcoreapp2.1/static/
cp alarts.example.json bin/Debug/netcoreapp2.1/alerts.example.json
cp config.example.json bin/Debug/netcoreapp2.1/config.example.json

echo "Changing directory to build folder..."
cd bin/debug/netcoreapp2.1