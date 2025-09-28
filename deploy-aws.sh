#!/bin/bash

# Script complet pentru deploy pe AWS
echo "🚀 Deploying GameImpostors to AWS..."

# 1. Configurează AWS IP-ul
echo "📝 Please update the following files with your AWS IP:"
echo "   - Frontend/src/services/api.ts"
echo "   - Frontend/src/components/LoginPage.tsx" 
echo "   - Frontend/src/components/RegisterPage.tsx"
echo "   - Backend/Program.cs (CORS origins)"
echo ""

# 2. Build frontend
echo "🔨 Building frontend..."
cd Frontend
npm install
npm run build
echo "✅ Frontend built successfully!"

# 3. Start backend
echo "🔧 Starting backend..."
cd ../Backend
dotnet restore
dotnet build
echo "✅ Backend ready to start!"

echo ""
echo "🎯 To start the application:"
echo ""
echo "Backend (Terminal 1):"
echo "  cd Backend"
echo "  dotnet run --launch-profile https"
echo ""
echo "Frontend (Terminal 2):"
echo "  cd Frontend"
echo "  npx serve -s dist -l 5173"
echo ""
echo "🌐 Access your app at: https://YOUR-AWS-IP:5173"
echo "🔒 Backend API: https://YOUR-AWS-IP:5087/api"
echo ""
echo "⚠️  Don't forget to:"
echo "   1. Replace 'your-aws-ip' with your actual AWS IP"
echo "   2. Open ports 5087 (backend) and 5173 (frontend) in AWS Security Groups"
echo "   3. Configure SSL certificates if needed"
