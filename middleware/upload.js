const multer = require("multer");
const path = require("path");

// Configure storage
const storage = multer.diskStorage({
  destination: (req, file, cb) => {
    cb(null, "uploads/");
  },
  filename: (req, file, cb) => {
    // For PDFs and videos, keep original filename
    if (file.mimetype === "application/pdf" || file.mimetype.startsWith("video/")) {
      cb(null, file.originalname);
    } else {
      // For images, generate unique filename with timestamp
      const uniqueSuffix = Date.now() + "-" + Math.round(Math.random() * 1e9);
      cb(null, file.fieldname + "-" + uniqueSuffix + path.extname(file.originalname));
    }
  },
});

// File filter for image, PDF, and video types
const fileFilter = (req, file, cb) => {
  // Check if file is an image, PDF, or video
  if (file.mimetype.startsWith("image/") || file.mimetype === "application/pdf" || file.mimetype.startsWith("video/")) {
    cb(null, true);
  } else {
    cb(new Error("Only image, PDF, and video files are allowed!"), false);
  }
};

// Configure multer
const upload = multer({
  storage: storage,
  fileFilter: fileFilter,
  limits: {
    fileSize: 5 * 1024 * 1024, // 5MB limit
  },
});

// Error handling middleware for multer
const handleUploadError = (error, req, res, next) => {
  if (error instanceof multer.MulterError) {
    if (error.code === "LIMIT_FILE_SIZE") {
      return res.status(400).json({
        success: false,
        message: "File too large. Maximum size is 5MB.",
      });
    }
    if (error.code === "LIMIT_FILE_COUNT") {
      return res.status(400).json({
        success: false,
        message: "Too many files. Maximum is 1 file per upload.",
      });
    }
  }

  if (error.message === "Only image, PDF, and video files are allowed!") {
    return res.status(400).json({
      success: false,
      message: error.message,
    });
  }

  next(error);
};

module.exports = {
  upload,
  handleUploadError,
};
