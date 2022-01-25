# Download .NET 5.0 installer
curl https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh > dotnet-install.sh
echo "Downloading .NET 5.0 installer..."

# Make installer executable
echo "Setting executable permissions..."
chmod +x dotnet-install.sh

# Install .NET 5.0 SDK
echo "Launching .NET installer..."
./dotnet-install.sh --version 5.0.404

# Delete .NET Core 5.0 installer
echo "Deleting .NET installer..."
rm dotnet-install.sh

# Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr -b v5-rewrite

# Change directory into cloned repository
echo "Changing directory..."
cd WhMgr

# Build WhMgr.dll
echo "Building WhMgr..."
~/.dotnet/dotnet build

# Copy example config
echo "Copying example files..."
cp examples/discord_auth.json bin/discord_auth.json
cp -R examples/configs bin/configs/
cp bin/configs/config.example.json bin/configs/config.json
cp -R examples/alarms bin/alarms/
cp -R examples/embeds bin/embeds/
cp -R examples/discords bin/discords/
cp -R examples/filters bin/filters/
cp -R examples/geofences bin/geofences/
cp -R static/ bin/static/
curl https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json > bin/static/data/masterfile.json

echo "Changing directory to build folder..."
cd bin
