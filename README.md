# ASP.NET Core + Vue.js Starter App

A full-stack web application template that serves both frontend and backend from a single URL.

## 🚀 Features

### Backend (ASP.NET Core 10.0)
- **Modern Web API** with Swagger/OpenAPI documentation
- **SAML Authentication** integration with Microsoft Azure AD
- **JWT Token Authentication** for API access
- **Entity Framework Core** with SQL Server integration
- **ASP.NET Identity** for user management
- **CORS Configuration** for frontend integration
- **HTTPS Support** with development certificates

### Frontend (Vue.js 3 + Vuetify)
- **Vue.js 3** with Composition API
- **Vuetify 3** Material Design component library with custom WCTC theming
- **Vue Router** for client-side routing
- **Vite** for lightning-fast development and building
- **Responsive Design** with mobile-first approach
- **SCSS Support** with custom variables and component styling
- **Custom Theme System** with light/dark mode support
- **WCTC Brand Integration** with official colors and typography

### DevOps & Deployment
- **Docker Support** with nginx reverse proxy for HTTPS
- **Multi-stage Docker builds** for optimized containers
- **Environment-based configuration** (.env files)
- **Azure App Service ready** deployment configuration
- **Database migration support** with Entity Framework

## 📋 Prerequisites

### For Local Development
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20.11.0+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### For Docker Development
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## 🛠️ Getting Started

### Option 1: Docker (Recommended)

#### Create your .env file from .env.example and configure for your database using a connection string
DB_CONNECTION=Server=your-server;Database=your-db;User ID=your-user;Password=your-password

#### Start the application
```docker-compose up --build -d```

#### Access the application
 🌐 Application: https://localhost:5001
 📊 API Documentation: https://localhost:5001/swagger


### Option 2: Non-Docker Development

#### 1. Install Dependencies
```bash
# Install .NET dependencies
dotnet restore
```

#### 2. Configure Database
Update `appsettings.json` with your database connection string:
```json
{
  "ConnectionStrings": {
    "Connection": "Server=your-server;Database=your-db;User ID=your-user;Password=your-password"
  }
}
```

#### 3. Run Database Migrations
```bash
# Create and apply migrations
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### 4. Run the Application
```bash
dotnet build

dotnet run
```
#### Access the application
 🌐 Application: https://localhost:5001
 📊 API Documentation: https://localhost:5001/swagger


## 🔐 Authentication System

### SAML Authentication
The application supports SAML authentication with Microsoft Azure AD:

- **Login Endpoint**: `/Authentication/loginWithSaml?forceLogin=false`
- **SAML Response Handler**: `/Authentication/consumeSaml`
- **Logout**: `/Authentication/logout`
- **Logout Callback**: `/Authentication/logoutCallback`
- **Test Endpoint**: `/Authentication/test` - Check authentication status

#### Authentication Flow
1. User initiates login via `/Authentication/loginWithSaml`
2. System redirects to Microsoft Azure AD SAML endpoint
3. User authenticates with Microsoft
4. SAML response is posted to `/Authentication/consumeSaml`
5. System validates SAML response and certificate
6. Extracts username (NameID) from SAML response
7. Searches database for matching user
8. **If user found**: Creates JWT token with roles/permissions and grants full access
9. **If user not found**: Creates temporary SAML session with limited access (temp role)

#### SAML Error Handling

The system includes error handling for SAML authentication:

**Certificate Validation Errors:**
- Invalid or missing SAML certificate → Creates temp user with `CertificateError` claim
- Expired certificate → Logs error and creates temp user
- Certificate format errors → Handles gracefully with fallback

**SAML Response Validation Errors:**
- Invalid SAML response → Falls back to extracting NameID from XML
- Missing NameID → Uses "unknown" as username, creates temp user
- Response validation failure → Creates temp user session

**User Not Found:**
- User authenticated via SAML but not in database → Creates temp user session
- Temp users have limited permissions (canViewHome only)
- Temp users are redirected to home page (`/`)

### JWT Token Authentication
For API access, the application uses JWT tokens:

- **Token Storage**: HTTP-only cookies
- **Token Expiration**: Configurable (default: 7 days)
- **Automatic Refresh**: Handled by the frontend
- **Claims**: Includes user ID and username

### User Management

The application provides user management with permission-based access control.

#### Features
- **User CRUD Operations**: Create, read, update, and deactivate users
- **Role Assignment**: Multiple roles per user supported
- **User Status**: Active/inactive via lockout mechanism
- **Profile Management**: Users can update their own profile
- **Permission-Based Access**: All operations require specific permissions

#### User Roles & Permissions

The system uses a role-based permission system defined in `src/shared/role-permissions.json`:

**Default Roles:**
- **temp**: Limited access (canViewHome only) - for SAML users not in database
- **User**: Basic access (canViewHome, canViewManageUsers)
- **Manager**: Enhanced access (canViewHome, canViewManageUsers, canEditUsers, canViewManageRoles, canEditRoles)
- **Admin**: Full access (all permissions including canAddUsers, canAddRoles)

**Permissions:**
- `canViewHome` - Access home page
- `canViewManageUsers` - View user management page
- `canViewManageRoles` - View role management page
- `canAddUsers` - Create new users
- `canEditUsers` - Edit existing users
- `canAddRoles` - Create new roles
- `canEditRoles` - Edit roles and permissions

#### User Operations

**Creating Users:**
- Requires `canAddUsers` permission
- Assigns default "User" role if none specified
- Supports multiple role assignment

**Updating Users:**
- Requires `canEditUsers` permission
- Can update: StudentId, FirstName, LastName, Email, UserName, PhoneNumber
- Can activate/deactivate users
- Can modify role assignments

### Role Management

The application provides full role management capabilities with permission assignment.

#### Features
- **Role CRUD**: Create, read, update, and delete roles
- **Permission Management**: Assign permissions to roles via JSON storage
- **Role Membership**: Add/remove users from roles
- **Permission-Based Access**: All operations require specific permissions

#### Role Operations

**Creating Roles:**
- Requires `canAddRoles` permission
- Validates for duplicate role names (case-insensitive)

**Updating Roles:**
- Requires `canEditRoles` permission
- Can rename roles
- Can update role permissions (stored as JSON in database)

**Deleting Roles:**
- Requires `canEditRoles` permission
- Automatically removes all users from role before deletion
- Prevents deletion if users cannot be removed

**Role Permissions:**
- Permissions stored as JSON array in `AspNetRoles.Permissions` field
- Managed via `PermissionService` class
- Supports multiple permissions per role
- Permissions aggregated across all user roles in JWT token

## 🐳 Docker Configuration

### Multi-Stage Build Architecture

The Dockerfile uses a multi-stage build process for optimized image size and build performance:

1. **Frontend Build Stage** (`frontend-build`): Builds Vue.js application using Node.js Alpine
2. **Base Runtime Stage** (`base`): Lightweight ASP.NET runtime image
3. **Build Stage** (`build`): .NET SDK for compiling the application
4. **Publish Stage** (`publish`): Publishes the application
5. **Final Stage** (`final`): Production-ready image

### Optimizations

- **Layer Caching**: Package files copied first for faster rebuilds
- **npm Optimization**: Uses `--prefer-offline --no-audit` for faster installs
- **.NET Build**: Uses `--no-restore` to avoid redundant package restores
- **Security**: Runs as non-root user (`appuser`) in final stage
- **Shared Resources**: Frontend and backend share `src/shared/` folder for role-permissions.json

### Services

#### nginx (Reverse Proxy)
- **Port**: 5001 (external) → 443 (internal HTTPS)
- **Purpose**: HTTPS termination and request forwarding
- **SSL Certificates**: Development certificates mounted from host

#### starter-app-dev (ASP.NET Core)
- **Port**: 8080 (internal HTTP)
- **Purpose**: Main application server
- **Environment**: Development with hot reloading
- **Database**: External SQL Server connection
- **Volumes**: 
  - Source code mounted for live development
  - Build artifacts excluded via anonymous volumes

### Docker Commands
```bash
# Start all services
docker-compose up --build

# Start in background
docker-compose up --build -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down -v

# Rebuild and start
docker-compose up --build --force-recreate
```

## 📁 Project Structure

```
Starter-App/
├── src/
│   ├── frontend/                    # Vue.js 3 + Vuetify frontend
│   │   ├── components/              # Reusable Vue components
│   │   │   ├── icons/              # Custom icon components
│   │   │   ├── roles/              # Role management components
│   │   │   ├── AddUserForm.vue     # User creation form
│   │   │   ├── EditUserForm.vue    # User editing form
│   │   │   ├── LogoutButton.vue    # Logout functionality
│   │   │   ├── NavBar.vue          # Navigation bar
│   │   │   ├── ThemeToggle.vue     # Theme toggle
│   │   │   ├── UserList.vue        # User list display
│   │   │   └── UserMenu.vue        # User menu component
│   │   ├── views/                  # Page components
│   │   │   ├── HomeView.vue        # Home page
│   │   │   ├── UserView.vue        # User dashboard
│   │   │   ├── UserManagementView.vue  # User management
│   │   │   └── RoleManagementView.vue  # Role management
│   │   ├── router/                 # Vue Router configuration
│   │   │   ├── index.js
│   │   │   ├── permissions.js     # Permission-based routing
│   │   │   └── useAuth.js          # Authentication composable
│   │   ├── plugins/                # Vuetify configuration
│   │   │   └── vuetify.js          # Theme setup & Vuetify initialization
│   │   ├── styles/                 # SCSS styles
│   │   │   ├── main.scss           # Global styles, component overrides
│   │   │   └── variables.scss      # SCSS variables for consistency
│   │   └── assets/                 # Static assets
│   ├── shared/                     # Shared resources
│   │   ├── role-permissions.json  # Role & permission definitions
│   │   └── types.js                # Shared TypeScript types
│   └── backend/                    # ASP.NET Core API
│       ├── Controllers/            # API controllers
│       │   ├── AuthenticationController.cs  # SAML/JWT auth
│       │   ├── ManageUserController.cs     # User management
│       │   └── RoleController.cs          # Role management
│       ├── Models/                 # Data models
│       │   ├── AppStarterContext.cs        # EF DbContext
│       │   ├── AspNetUser.cs              # User model
│       │   ├── AspNetRole.cs              # Role model
│       │   ├── AuthRequest.cs             # Auth request model
│       │   ├── SamlResponse.cs            # SAML response model
│       │   ├── Auth/                      # Auth models
│       │   └── DTOs/                      # Data Transfer Objects
│       ├── Services/               # Business logic services
│       │   └── PermissionService.cs
│       └── Attributes/            # Custom attributes
│           └── RequirePermissionAttribute.cs
├── wwwroot/                        # Built frontend (auto-generated)
├── Migrations/                     # Entity Framework migrations
├── Properties/
│   └── launchSettings.json        # Local development settings
├── src/Dockerfile                  # Multi-stage Docker build
├── docker-compose.yml             # Docker services configuration
├── nginx.conf                     # nginx reverse proxy config
├── nginx-startup.sh               # nginx startup script
├── appsettings.json               # Application configuration
└── .env                           # Environment variables (create this)
```

## 🔧 Development Workflow

### Frontend Development
```bash
cd src/frontend

# Development server (if running separately)
npm run dev

# Build for production
npm run build

# Lint and fix code
npm run lint

# Clean build artifacts
npm run clean
```

#### Theme Development
When working with themes and styling:

```bash
# Watch for SCSS changes during development
npm run dev  # Vite automatically handles hot reloading for styles

# Validate theme colors in browser dev tools
# CSS custom properties are available as --v-theme-[colorname]

# Testing dark mode
# Toggle between themes in browser or through Vuetify's theme switching
```

### Backend Development
```bash
# Run with hot reload
dotnet watch run

# Build project
dotnet build

# Run tests
dotnet test

# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

### Database Management
```bash
# Create new migration
dotnet ef migrations add DescriptionOfChanges

# Apply migrations
dotnet ef database update

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script
```

## 🔍 API Endpoints

### Authentication
- **GET** `/Authentication/loginWithSaml` - Initiate SAML login
- **POST** `/Authentication/consumeSaml` - Handle SAML response
- **GET** `/Authentication/logout` - Logout user
- **GET** `/Authentication/test` - Test authentication status

### User Management
- **GET** `/api/users` - List all users
- **GET** `/api/users/{id}` - Get user by ID
- **POST** `/api/users` - Create new user
- **PUT** `/api/users/{id}` - Update user
- **PATCH** `/api/users/{id}/deactivate` - Deactivate user
- **PATCH** `/api/users/{id}/reactivate` - Reactivate user
- **GET** `/api/profile` - Get current user profile
- **PUT** `/api/profile` - Update current user profile

### Role Management
- **GET** `/Role` - List all roles
- **POST** `/Role` - Create new role
- **PUT** `/Role/{roleId}` - Update role
- **DELETE** `/Role/{roleId}` - Delete role
- **GET** `/Role/{roleId}/users` - Get users in role
- **POST** `/Role/{roleId}/users/{userId}` - Add user to role
- **DELETE** `/Role/{roleId}/users/{userId}` - Remove user from role
- **GET** `/Role/{roleId}/permissions` - Get role permissions
- **PUT** `/Role/{roleId}/permissions` - Update role permissions

### API Documentation
- **GET** `/swagger` - Interactive API documentation (development only)

## 🎨 Vuetify Setup & Theming

### Theme Configuration

The application uses a custom Vuetify theme that incorporates WCTC's official brand colors and provides both light and dark mode support.

#### Theme Colors

**Primary Brand Colors:**
- **Primary**: `#003E7A` (WCTC Navy Blue) - Main brand color
- **Secondary**: `#0077C8` (Bright Blue) - Interactive elements
- **Accent**: `#FFD54F` (Accent Yellow) - Highlights and call-to-action
- **Teal**: `#00A6A6` (Accent Teal) - Success states and secondary actions
- **Purple**: `#5F6DB3` (Accent Purple) - Info states and accents

**Surface & Background:**
- **Surface**: `#F4F6F8` (Light gray-blue) - Cards and elevated content
- **Background**: `#FFFFFF` (Pure white) - Page background
- **Border**: `#DDE3EB` (Light gray) - Dividers and borders

#### Theme Implementation

The theme is configured in `src/frontend/plugins/vuetify.js`:

```javascript
// Custom theme configuration
const vuetify = createVuetify({
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#003E7A',    // WCTC Navy Blue
          secondary: '#0077C8',  // Bright Blue
          accent: '#FFD54F',     // Accent Yellow
          // ... additional colors
        },
      },
      dark: {
        colors: {
          primary: '#0077C8',    // Brighter primary for dark mode
          secondary: '#5F6DB3',  // Adjusted secondary
          // ... dark mode variants
        },
      },
    },
  },
})
```

### Styling Architecture

#### File Structure
```
src/frontend/styles/
├── main.scss              # Main stylesheet with global styles
├── variables.scss         # SCSS variables for consistency
```

#### Typography
- **Font Family**: Karla (Google Fonts) - Clean, modern sans-serif
- **Font Weights**: 400 (regular), 600 (semi-bold), 700 (bold)
- **Implementation**: `@import url('https://fonts.googleapis.com/css2?family=Karla:wght@400;600;700&display=swap')`

#### Global Style Features

**CSS Custom Properties Integration:**
The styling system leverages Vuetify's CSS custom properties for theme-aware styling:

```scss
// Example usage in main.scss
body {
  color: rgb(var(--v-theme-on-background));
  background-color: rgb(var(--v-theme-background));
}

h1, h2, h3, h4, h5, h6 {
  color: rgb(var(--v-theme-primary));
}
```

**Component Style Overrides:**
Custom styling for Vuetify components to match brand guidelines:

```scss
.v-btn {
  text-transform: uppercase;
  font-weight: 600;
  letter-spacing: 0.5px;
  border-radius: 8px;
}
```

### SCSS Variables System

Located in `src/frontend/styles/variables.scss`:

```scss
// Layout
$max-width: 1200px;
$spacing-unit: 1rem;

// Typography
$font-family-primary: 'Karla', Arial, Helvetica, sans-serif;
$font-size-base: 1rem;
```

### Vite Configuration for Styling

The project uses modern SCSS compilation in `vite.config.js`:

```javascript
css: {
  preprocessorOptions: {
    scss: {
      api: 'modern-compiler',
    },
  }
}
```

### Using the Theme System

#### In Vue Components
```vue
<!-- Using theme colors in templates -->
<template>
  <v-btn color="primary">Primary Action</v-btn>
  <v-btn color="secondary">Secondary Action</v-btn>
  <v-card color="surface">Content Card</v-card>
</template>

<style scoped>
/* Using CSS custom properties */
.custom-element {
  background-color: rgb(var(--v-theme-accent));
  border: 1px solid rgb(var(--v-theme-border));
}
</style>
```

#### Available Theme Tokens
All theme colors are available as CSS custom properties:
- `--v-theme-primary`
- `--v-theme-secondary`
- `--v-theme-accent`
- `--v-theme-surface`
- `--v-theme-background`
- `--v-theme-success`
- `--v-theme-warning`
- `--v-theme-error`
- `--v-theme-info`

### Dependencies

Key styling and theming dependencies in `package.json`:

```json
{
  "dependencies": {
    "vuetify": "^3.10.2",
    "vite-plugin-vuetify": "^2.1.2"
  },
  "devDependencies": {
    "@mdi/font": "^7.4.47",    // Material Design Icons
    "sass": "^1.92.1",          // SCSS compiler
    "sass-loader": "^16.0.5"    // SCSS loader for Vite
  }
}
```

### Customization Guidelines

When extending or modifying the theme:

1. **Brand Consistency**: Always use the defined brand colors from the theme
2. **Accessibility**: Ensure sufficient contrast ratios (especially in dark mode)
3. **Responsive Design**: Use Vuetify's breakpoint system for responsive styling
4. **CSS Custom Properties**: Prefer theme tokens over hardcoded colors
5. **Component Overrides**: Place component-specific overrides in `main.scss`

### Dark Mode Support

The application includes a complete dark theme that automatically adjusts:
- Background and surface colors for better visibility
- Adjusted primary/secondary colors for dark backgrounds
- Maintained brand identity while ensuring usability

Users can toggle between light and dark themes, and the choice persists across sessions.

## 🔧 Configuration

### Environment Variables

Create a `.env` file in the root directory:

```env
# Database Connection
DB_CONNECTION=Server=your-server;Database=your-db;User ID=your-user;Password=your-password

# SAML Configuration (already configured in appsettings.json)
# BaseURL=https://localhost:5001
# SamlEndpoint=https://login.microsoftonline.com/your-tenant/saml2
# SamlAssertionURL=https://localhost:5001/Authentication/consumeSaml
```

### appsettings.json Configuration

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "Connection": "Your database connection string"
  },
  "Jwt": {
    "Key": "Your JWT signing key",
    "ValidFor": 7
  },
  "SsoIssuerApplicationName": "Your SAML issuer name",
  "BaseURL": "https://localhost:5001",
  "SamlEndpoint": "Your SAML endpoint URL",
  "SamlCert": "Your SAML certificate"
}
```


## 🔧 Troubleshooting

### Common Issues

#### Docker Issues
- **Port conflicts**: Ensure ports 5001 and 8080 are available
- **Certificate issues**: Run `dotnet dev-certs https --trust`
- **Database connection**: Verify `.env` file has correct connection string

#### Vuetify/Styling Issues
- **Theme not applying**: Check that `vuetify.js` is properly imported in `main.js`
- **SCSS compilation errors**: Verify Vite configuration for SCSS preprocessor
- **Custom properties not working**: Ensure you're using `rgb(var(--v-theme-colorname))` format
- **Font not loading**: Check Google Fonts import in `main.scss`

#### Database Issues
- **Migration errors**: Check connection string and database permissions
- **Entity Framework**: Ensure all models are properly configured in `AppStarterContext`

#### SAML Issues
- **Certificate problems**: 
  - Verify SAML certificate format in `appsettings.json`
  - Check certificate expiration dates (system logs errors for expired certs)
  - Invalid certificates result in temp user creation with `CertificateError` claim
- **Redirect URLs**: Ensure SAML configuration matches your application URL
- **Temp users**: Users authenticated via SAML but not in database get temp role with limited access
- **SAML response errors**: System falls back to extracting NameID from XML if validation fails
- **User not found**: SAML-authenticated users not in database are automatically granted temp access

### Getting Help

1. Check the logs: `docker-compose logs -f`
2. Verify configuration files
3. Test database connectivity
4. Check SAML configuration with your identity provider

## 📚 Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Vue.js 3 Guide](https://vuejs.org/guide/)
- [Vuetify Component Library](https://vuetifyjs.com/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Docker Documentation](https://docs.docker.com/)#   W W T - N o r t h w i n d  
 