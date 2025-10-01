const mongoose = require("mongoose");
const bcrypt = require("bcryptjs");
require("dotenv").config();

const Admin = require("./models/Admin");

const setupDatabase = async () => {
  try {
    // Connect to MongoDB
    console.log("🔌 Connecting to MongoDB...");
    await mongoose.connect(process.env.MONGODB_URI, {
      useNewUrlParser: true,
      useUnifiedTopology: true,
    });
    console.log("✅ MongoDB Connected Successfully!");

    // Check if admin already exists
    const existingAdmin = await Admin.findOne({ username: process.env.ADMIN_USERNAME });

    if (existingAdmin) {
      console.log("ℹ️  Admin user already exists:", process.env.ADMIN_USERNAME);
      console.log("📊 Database setup complete!");
    } else {
      // Create default admin user
      console.log("👤 Creating default admin user...");

      const saltRounds = 12;
      const hashedPassword = await bcrypt.hash(process.env.ADMIN_PASSWORD, saltRounds);

      const admin = new Admin({
        username: process.env.ADMIN_USERNAME,
        password: hashedPassword,
      });

      await admin.save();
      console.log("✅ Admin user created successfully!");
      console.log("📊 Database setup complete!");
    }

    // Display connection info
    console.log("\n📋 Connection Information:");
    console.log(`   Database: ${mongoose.connection.name}`);
    console.log(`   Host: ${mongoose.connection.host}`);
    console.log(`   Port: ${mongoose.connection.port}`);
    console.log(`   Admin Username: ${process.env.ADMIN_USERNAME}`);
    console.log(`   Server Port: ${process.env.PORT}`);
    console.log(`   Environment: ${process.env.NODE_ENV}`);

    // Test database operations
    console.log("\n🧪 Testing database operations...");
    const adminCount = await Admin.countDocuments();
    console.log(`   Total admins: ${adminCount}`);

    const allAdmins = await Admin.find().select("username createdAt");
    console.log("   Admin users:");
    allAdmins.forEach((admin) => {
      console.log(`     - ${admin.username} (created: ${admin.createdAt.toISOString()})`);
    });

    console.log("\n🎉 Setup completed successfully!");
    console.log("🚀 You can now start the server with: npm start or npm run dev");
  } catch (error) {
    console.error("❌ Setup failed:", error.message);
    process.exit(1);
  } finally {
    // Close connection
    await mongoose.connection.close();
    console.log("🔌 Database connection closed.");
  }
};

// Run setup
setupDatabase();
