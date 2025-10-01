# MIC CPanel - Website Control Panel API

A comprehensive Node.js API for managing MIC (Al Mohandes Insurance Company) website content with multilingual support (English/Arabic) and file upload capabilities.

## 🚀 Features

- **Multilingual Content Management** - Support for English and Arabic content
- **Page Management** - Create, read, update, and delete website pages
- **Form Management** - Handle contact forms and other user submissions
- **File Upload** - Image and document upload with validation
- **Admin Authentication** - Secure admin panel with JWT authentication
- **Template System** - Pre-built templates for various insurance products
- **Security** - Rate limiting, CORS, Helmet security headers
- **Database** - MongoDB with Mongoose ODM

## 📋 Prerequisites

- Node.js (v14 or higher)
- MongoDB (local or cloud instance)
- npm or yarn

## 🛠️ Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd mic-cpanel
   ```

2. **Install dependencies**

   ```bash
   npm install
   ```

3. **Environment Setup** Create a `.env` file in the root directory:

   ```env
   # Database
   MONGODB_URI=mongodb://localhost:27017/mic-cpanel

   # Server
   PORT=5000
   NODE_ENV=development

   # Admin Credentials
   ADMIN_USERNAME=admin
   ADMIN_PASSWORD=your_secure_password

   # JWT Secret
   JWT_SECRET=your_jwt_secret_key
   ```

4. **Database Setup** Run the setup script to initialize the database and create admin user:

   ```bash
   node setup.js
   ```

5. **Start the server**

   ```bash
   # Development mode with auto-reload
   npm run dev

   # Production mode
   npm start
   ```

## 📚 API Endpoints

### Authentication

- `POST /api/auth/login` - Admin login
- `POST /api/auth/logout` - Admin logout
- `GET /api/auth/verify` - Verify JWT token

### Pages Management

- `GET /api/pages` - Get all pages
- `GET /api/pages/:id` - Get specific page
- `POST /api/pages` - Create new page
- `PUT /api/pages/:id` - Update page
- `DELETE /api/pages/:id` - Delete page

### Forms Management

- `GET /api/forms` - Get all forms
- `GET /api/forms/:id` - Get specific form
- `POST /api/forms` - Create new form
- `PUT /api/forms/:id` - Update form
- `DELETE /api/forms/:id` - Delete form

### Admin Management

- `GET /api/admin/profile` - Get admin profile
- `PUT /api/admin/profile` - Update admin profile
- `POST /api/admin/change-password` - Change admin password

### Health Check

- `GET /` - API status and information
- `GET /health` - Detailed health check with database status

## 🏗️ Project Structure

```
mic-cpanel/
├── middleware/
│   ├── auth.js          # JWT authentication middleware
│   └── upload.js        # File upload middleware
├── models/
│   ├── Admin.js         # Admin user model
│   ├── Form.js          # Form submission model
│   └── Page.js          # Page content model
├── routes/
│   ├── admin.js         # Admin management routes
│   ├── auth.js          # Authentication routes
│   ├── forms.js         # Form management routes
│   └── pages.js         # Page management routes
├── templates/
│   ├── homepage_template.json
│   ├── about_us_page_template.json
│   ├── car_Insurance_template.json
│   └── ...              # Various insurance product templates
├── uploads/             # Uploaded files directory
├── ssl/                 # SSL certificates (excluded from git)
├── server.js            # Main server file
├── setup.js             # Database initialization script
└── package.json
```

## 🌐 Multilingual Support

The application supports both English and Arabic content through a structured schema:

```json
{
  "id": "unique_identifier",
  "en": "English content",
  "ar": "المحتوى باللغة العربية"
}
```

## 📁 File Upload

- **Supported formats**: Images (jpg, jpeg, png, gif), Documents (pdf, doc, docx)
- **Upload endpoints**: `/api/pages/upload` and `/api/forms/upload`
- **File storage**: Local `uploads/` directory
- **Access**: Files served at `/uploads/filename`

## 🔒 Security Features

- **Rate Limiting**: 100 requests per 15 minutes per IP
- **CORS**: Configurable cross-origin resource sharing
- **Helmet**: Security headers protection
- **JWT Authentication**: Secure token-based authentication
- **Password Hashing**: bcrypt with 12 salt rounds
- **Input Validation**: Request body size limits and validation

## 🚀 Deployment

### Production Environment Variables

```env
NODE_ENV=production
MONGODB_URI=mongodb+srv://username:password@cluster.mongodb.net/mic-cpanel
PORT=5000
ADMIN_USERNAME=your_admin_username
ADMIN_PASSWORD=your_secure_password
JWT_SECRET=your_production_jwt_secret
```

### SSL Configuration

Place your SSL certificates in the `ssl/` directory:

- `ssl/cert.pem` - SSL certificate
- `ssl/key.pem` - Private key

## 🧪 Testing

Test the API endpoints using tools like Postman or curl:

```bash
# Health check
curl http://localhost:5000/health

# Login
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"your_password"}'
```

## 📝 Templates

The application includes pre-built templates for various insurance products:

- Homepage
- About Us
- Car Insurance
- Medical Insurance
- Property Insurance
- Engineering Insurance
- Personal Accident Insurance
- Transport Insurance
- Board of Directors
- Investor Relations
- Media Center
- Social Responsibility

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the ISC License.

## 🆘 Support

For support and questions, please contact the development team or create an issue in the repository.

---

**MIC CPanel v1.0.0** - Built with ❤️ for Al Mohandes Insurance Company
