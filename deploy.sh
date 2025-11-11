#!/bin/bash

# 🚀 Script de Deployment Complet pentru AWS Ubuntu EC2
# Acest script automatizează tot procesul de deployment

set -e  # Stop dacă întâlnește erori

# Culori pentru output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}═════════════════════════════════════════════${NC}"
echo -e "${BLUE}   🚀 AuthService - Deployment Script       ${NC}"
echo -e "${BLUE}═════════════════════════════════════════════${NC}"

# Verifică dacă rulează pe Ubuntu
if ! command -v lsb_release &> /dev/null || [[ $(lsb_release -si) != "Ubuntu" ]]; then
    echo -e "${RED}❌ Acest script este pentru Ubuntu!${NC}"
    exit 1
fi

# ========== PASUL 1: Instalare Dependențe ==========
echo -e "\n${YELLOW}📦 PASUL 1: Instalare Dependențe...${NC}"

# Update system
echo -e "${BLUE}→ Update sistem...${NC}"
sudo apt update && sudo apt upgrade -y

# .NET 8.0
if ! command -v dotnet &> /dev/null; then
    echo -e "${BLUE}→ Instalare .NET 8.0 SDK...${NC}"
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
    chmod +x dotnet-install.sh
    ./dotnet-install.sh --channel 8.0
    rm dotnet-install.sh
    
    # Add to PATH
    export DOTNET_ROOT=$HOME/.dotnet
    export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools
    echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
    echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
    
    echo -e "${GREEN}✓ .NET 8.0 instalat cu succes!${NC}"
else
    echo -e "${GREEN}✓ .NET 8.0 deja instalat${NC}"
fi

# Node.js
if ! command -v node &> /dev/null; then
    echo -e "${BLUE}→ Instalare Node.js 20.x...${NC}"
    curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash -
    sudo apt install -y nodejs
    echo -e "${GREEN}✓ Node.js instalat cu succes!${NC}"
else
    echo -e "${GREEN}✓ Node.js deja instalat${NC}"
fi

# PM2
if ! command -v pm2 &> /dev/null; then
    echo -e "${BLUE}→ Instalare PM2...${NC}"
    sudo npm install -g pm2
    echo -e "${GREEN}✓ PM2 instalat cu succes!${NC}"
else
    echo -e "${GREEN}✓ PM2 deja instalat${NC}"
fi

# ========== PASUL 2: Configurare IP EC2 ==========
echo -e "\n${YELLOW}🌐 PASUL 2: Configurare IP EC2...${NC}"

# Obține IP-ul public al EC2
EC2_PUBLIC_IP=$(curl -s http://169.254.169.254/latest/meta-data/public-ipv4 2>/dev/null || echo "")

if [ -z "$EC2_PUBLIC_IP" ]; then
    echo -e "${RED}⚠️  Nu pot detecta automat IP-ul EC2!${NC}"
    read -p "Introdu manual IP-ul public al EC2: " EC2_PUBLIC_IP
fi

echo -e "${GREEN}✓ IP EC2 detectat: ${EC2_PUBLIC_IP}${NC}"

# ========== PASUL 3: Configurare Backend ==========
echo -e "\n${YELLOW}⚙️  PASUL 3: Configurare Backend...${NC}"

cd Backend

# Actualizează appsettings.Production.json cu IP-ul corect
echo -e "${BLUE}→ Actualizare appsettings.Production.json...${NC}"
sed -i "s|YOUR_EC2_IP|${EC2_PUBLIC_IP}|g" appsettings.Production.json

# Restaurează și build
echo -e "${BLUE}→ Restaurare dependențe Backend...${NC}"
dotnet restore

echo -e "${BLUE}→ Build Backend (Release)...${NC}"
dotnet build --configuration Release

# Migrații database
echo -e "${BLUE}→ Aplicare migrații database...${NC}"
dotnet ef database update || echo -e "${YELLOW}⚠️  Migrațiile nu au putut fi aplicate automat${NC}"

echo -e "${GREEN}✓ Backend configurat cu succes!${NC}"

# ========== PASUL 4: Configurare Frontend ==========
echo -e "\n${YELLOW}🎨 PASUL 4: Configurare Frontend...${NC}"

cd ../Frontend

# Creează .env.production cu IP-ul corect
echo -e "${BLUE}→ Creare .env.production...${NC}"
cat > .env.production << EOF
VITE_API_BASE_URL=http://${EC2_PUBLIC_IP}:5086/api
EOF

# Instalează și build
echo -e "${BLUE}→ Instalare dependențe Frontend...${NC}"
npm install

echo -e "${BLUE}→ Build Frontend (Production)...${NC}"
npm run build

echo -e "${GREEN}✓ Frontend configurat cu succes!${NC}"

# ========== PASUL 5: Creare scripturi de pornire ==========
echo -e "\n${YELLOW}📝 PASUL 5: Creare scripturi de pornire...${NC}"

cd ..

# Script Backend
cat > Backend/start-backend.sh << 'BACKEND_SCRIPT'
#!/bin/bash
cd ~/AuthService/Backend
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://0.0.0.0:5086
dotnet run --configuration Release
BACKEND_SCRIPT

chmod +x Backend/start-backend.sh
echo -e "${GREEN}✓ Script Backend creat${NC}"

# Script Frontend  
cat > Frontend/start-frontend.sh << 'FRONTEND_SCRIPT'
#!/bin/bash
cd ~/AuthService/Frontend
npx vite --host 0.0.0.0 --port 5173
FRONTEND_SCRIPT

chmod +x Frontend/start-frontend.sh
echo -e "${GREEN}✓ Script Frontend creat${NC}"

# ========== PASUL 6: Publish Backend pentru Production ==========
echo -e "\n${YELLOW}📦 PASUL 6: Publish Backend...${NC}"

cd Backend
echo -e "${BLUE}→ Build Backend ca DLL...${NC}"
$HOME/.dotnet/dotnet publish -c Release -o ./publish

echo -e "${GREEN}✓ Backend published cu succes!${NC}"

# ========== PASUL 7: Pornire cu PM2 ==========
echo -e "\n${YELLOW}🚀 PASUL 7: Pornire aplicații cu PM2...${NC}"

cd ..

# Stop aplicațiile dacă rulează deja
pm2 stop authservice-backend 2>/dev/null || true
pm2 stop authservice-frontend 2>/dev/null || true
pm2 delete authservice-backend 2>/dev/null || true
pm2 delete authservice-frontend 2>/dev/null || true

# Start Backend cu DLL
echo -e "${BLUE}→ Pornire Backend (Port 5086)...${NC}"
pm2 start $HOME/.dotnet/dotnet --name authservice-backend \
  --cwd ~/AuthService/Backend \
  -- ~/AuthService/Backend/publish/Backend.dll \
  --urls "http://0.0.0.0:5086" \
  --environment Production

# Așteaptă puțin ca backend să pornească
sleep 5

# Start Frontend cu vite preview (servește build-ul de production)
echo -e "${BLUE}→ Pornire Frontend (Port 5173)...${NC}"
pm2 start npx --name authservice-frontend \
  --cwd ~/AuthService/Frontend \
  -- vite preview --host 0.0.0.0 --port 5173

# Salvează configurația PM2
pm2 save
pm2 startup > /dev/null 2>&1

echo -e "${GREEN}✓ Aplicații pornite cu succes!${NC}"

# ========== FINAL ==========
echo -e "\n${GREEN}═════════════════════════════════════════════${NC}"
echo -e "${GREEN}   ✅ DEPLOYMENT COMPLET!                    ${NC}"
echo -e "${GREEN}═════════════════════════════════════════════${NC}"
echo -e ""
echo -e "${BLUE}📊 Status aplicații:${NC}"
pm2 status

echo -e "\n${BLUE}🌐 URL-uri accesibile:${NC}"
echo -e "   ${GREEN}Backend API:${NC} http://${EC2_PUBLIC_IP}:5086"
echo -e "   ${GREEN}Frontend:${NC}    http://${EC2_PUBLIC_IP}:5173"
echo -e "   ${GREEN}Swagger:${NC}     http://${EC2_PUBLIC_IP}:5086/swagger"
echo -e ""
echo -e "${YELLOW}📝 Comenzi utile:${NC}"
echo -e "   ${BLUE}pm2 status${NC}              - Status aplicații"
echo -e "   ${BLUE}pm2 logs${NC}                - Vezi toate logs"
echo -e "   ${BLUE}pm2 logs authservice-backend${NC}  - Logs Backend"
echo -e "   ${BLUE}pm2 logs authservice-frontend${NC} - Logs Frontend"
echo -e "   ${BLUE}pm2 restart all${NC}         - Restart toate"
echo -e "   ${BLUE}pm2 stop all${NC}            - Stop toate"
echo -e ""
echo -e "${GREEN}🎉 Gata! Aplicația ta rulează!${NC}\n"

