# Pull latest Git repository
echo "Pulling latest Git repository changes..."
git pull

# Build WhMgr.dll
echo "Building WhMgr..."
~/.dotnet/dotnet build

# Copy example config
#echo "Copying example files..."
#cp -R examples/alerts bin/debug/netcoreapp2.1/alerts/
#cp -R examples/filters bin/debug/netcoreapp2.1/filters/
#cp -R examples/templates bin/debug/netcoreapp2.1/templates/

# Copy locale translation files
echo "Copying locale translation files..."
cp -R static/locale bin/debug/netcoreapp2.1/static/locale

echo "Copying latest master file..."
cp static/data/masterfile.json bin/debug/netcoreapp2.1/static/data/masterfile.json
cp static/data/cpMultipliers.json bin/debug/netcoreapp2.1/static/data/cpMultipliers.json

echo "Update Complete"