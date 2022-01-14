# Download .NET 5.0 installer
echo "Downloading .NET 5.0 installer..."
curl https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh > dotnet-install.sh

# Make installer executable
echo "Setting executable permissions..."
chmod +x dotnet-install.sh

# Install .NET 5.0
echo "Launching .NET installer..."
./dotnet-install.sh --version 5.0.202

# Delete .NET 5.0 installer
echo "Deleting .NET installer..."
rm dotnet-install.sh

# Clone repository
echo "Cloning repository..."
git clone https://github.com/versx/WhMgr

# Change directory into cloned repository
echo "Changing directory..."
cd WhMgr

# Build WhMgr.dll
echo "Building WhMgr..."
~/.dotnet/dotnet build

# Copy example config
echo "Copying example files..."
cp -R examples/alarms bin/alarms/
cp -R examples/alerts bin/alerts/
cp -R examples/discords bin/discords/
cp -R examples/filters bin/filters/
cp -R examples/geofences bin/geofences/
cp -R static/ bin/static/
cp config.example.json bin/config.json
curl https://raw.githubusercontent.com/WatWowMap/Masterfile-Generator/master/master-latest.json > bin/static/data/masterfile.json

echo "Changing directory to build folder..."
cd bin
