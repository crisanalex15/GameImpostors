# 📁 Controllers Structure

## 🗂️ Organizarea Controller-elor

### 🔐 `/Auth/` - Authentication Controllers

- **`AuthController.cs`** - Basic authentication (login, register, verify, reset password)
- **`SocialAuthController.cs`** - Social authentication (Google, Facebook OAuth2)

### 👑 `/Admin/` - Administration Controllers

- **`UserManagementController.cs`** - User management, roles, admin functions

### 🔌 `/API/` - API Controllers

- **`AiController.cs`** - AI/OpenAI services
- **`HealthController.cs`** - Health checks and system status
- **`SocialAuthTestController.cs`** - Social auth testing endpoints
- **`SwaggerController.cs`** - API documentation
- **`TestController.cs`** - Development testing endpoints

### 🌐 `/Web/` - MVC Web Controllers

- **`HomeController.cs`** - Main website pages
- **`DashboardController.cs`** - User dashboard views

## 🎯 Conventions

- **API Controllers**: Use `[ApiController]` and return JSON
- **Web Controllers**: Use `Controller` and return Views
- **Auth Controllers**: Handle authentication flows
- **Admin Controllers**: Require `[Authorize(Roles = "Admin")]`

## 🔗 Routing

- **API**: `/api/[controller]/[action]`
- **Web**: `/{controller}/{action}/{id?}`
- **Auth**: `/api/auth/*` and `/api/socialauth/*`
- **Admin**: `/admin/*` (if using area routing)
