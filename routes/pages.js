const express = require("express");
const router = express.Router();
const mongoose = require("mongoose");
const { upload, handleUploadError } = require("../middleware/upload");

// GET /api/pages - Get all pages
router.get("/", async (req, res) => {
  try {
    const db = mongoose.connection.db;
    const collection = db.collection("pages");
    const pages = await collection.find().sort({ createdAt: -1 }).toArray();
    res.json({
      success: true,
      data: pages,
      count: pages.length,
    });
  } catch (error) {
    console.error("Error fetching pages:", error);
    res.status(500).json({
      success: false,
      message: "Error fetching pages",
      error: error.message,
    });
  }
});

// GET /api/pages/:id - Get a specific page by MongoDB _id
router.get("/:page", async (req, res) => {
  try {
    const mongoose = require("mongoose");
    const db = mongoose.connection.db;
    const collection = db.collection("pages");
    // const page = await collection.findOne({ _id: new mongoose.Types.ObjectId(req.params.id) });

    const page = await collection.findOne({ page: req.params.page });
    if (!page) {
      return res.status(404).json({
        message: "Page not found",
      });
    }

    res.json(page);
  } catch (error) {
    console.error("Error fetching page:", error);
    res.status(500).json({
      message: "Error fetching page",
      error: error.message,
    });
  }
});

// POST /api/pages - Create a new page
router.post("/", upload.array("images", 10), handleUploadError, async (req, res) => {
  try {
    const { page, sections } = req.body;

    // Process uploaded images
    let imageUrls = [];
    if (req.files && req.files.length > 0) {
      imageUrls = req.files.map((file) => `uploads/${file.filename}`);
    }

    // Parse sections data if it's a string
    let parsedSections = sections;
    if (typeof sections === "string") {
      try {
        parsedSections = JSON.parse(sections);
      } catch (e) {
        return res.status(400).json({
          success: false,
          message: "Invalid sections JSON format",
        });
      }
    }

    // Process images with sections data
    if (parsedSections && imageUrls.length > 0) {
      parsedSections = processImagesWithSections(parsedSections, imageUrls);
    }

    const db = mongoose.connection.db;
    const collection = db.collection("pages");

    const newPage = {
      page,
      sections: parsedSections,
      images: imageUrls, // Include uploaded images for backward compatibility
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    const result = await collection.insertOne(newPage);
    const savedPage = { ...newPage, _id: result.insertedId };

    res.status(201).json({
      success: true,
      message: "Page created successfully",
      data: savedPage,
    });
  } catch (error) {
    console.error("Error creating page:", error);
    res.status(500).json({
      success: false,
      message: "Error creating page",
      error: error.message,
    });
  }
});

// Helper function to process and merge uploaded images with sections data
const processImagesWithSections = (sections, uploadedImages, imageIndex = 0) => {
  if (!sections || !uploadedImages || uploadedImages.length === 0) {
    return sections;
  }

  const processedSections = JSON.parse(JSON.stringify(sections)); // Deep clone

  // Function to recursively find and replace image URLs
  const replaceImages = (obj, currentIndex = { value: imageIndex }) => {
    if (typeof obj === "object" && obj !== null) {
      if (Array.isArray(obj)) {
        obj.forEach((item) => replaceImages(item, currentIndex));
      } else {
        Object.keys(obj).forEach((key) => {
          if (key === "image" && typeof obj[key] === "string" && obj[key].includes("placeholder")) {
            // Replace placeholder images with uploaded ones
            if (currentIndex.value < uploadedImages.length) {
              obj[key] = uploadedImages[currentIndex.value];
              currentIndex.value++;
            }
          } else if (typeof obj[key] === "object") {
            replaceImages(obj[key], currentIndex);
          }
        });
      }
    }
  };

  replaceImages(processedSections);
  return processedSections;
};

// PUT /api/pages/:id - Update a page
router.put("/:id", upload.array("images", 10), handleUploadError, async (req, res) => {
  try {
    const mongoose = require("mongoose");
    const db = mongoose.connection.db;
    const collection = db.collection("pages");

    // Get existing page to check if it exists
    const existingPage = await collection.findOne({ _id: new mongoose.Types.ObjectId(req.params.id) });
    if (!existingPage) {
      return res.status(404).json({
        success: false,
        message: "Page not found",
      });
    }

    // Process images with sections data
    let sections = null;
    if (req.body.sections) {
      sections = processImagesWithSections(JSON.parse(req.body.sections), req.files || []);
    }

    // Prepare update data
    const updateData = {
      ...req.body,
      updatedAt: new Date(),
    };

    // Remove _id if present
    delete updateData._id;

    // Handle sections separately to avoid conflicts
    if (sections) {
      updateData.sections = sections;
    }

    // Build the $set operation for nested updates
    const setOperation = {};

    // Handle nested field updates
    Object.keys(updateData).forEach((key) => {
      if (key.includes(".")) {
        // This is a nested field update
        setOperation[key] = updateData[key];
        delete updateData[key];
      }
    });

    // Add remaining fields to setOperation
    Object.keys(updateData).forEach((key) => {
      if (key !== "_id") {
        setOperation[key] = updateData[key];
      }
    });

    const updatedPage = await collection.findOneAndUpdate(
      { _id: new mongoose.Types.ObjectId(req.params.id) },
      { $set: setOperation },
      { returnDocument: "after" }
    );

    if (updatedPage) {
      res.status(200).json({
        success: true,
        message: "Page updated successfully",
        data: updatedPage,
      });
    } else {
      res.status(404).json({
        success: false,
        message: "Page not found",
      });
    }
  } catch (error) {
    console.error("Error updating page:", error);
    res.status(500).json({
      success: false,
      message: "Error updating page",
      error: error.message,
    });
  }
});

// DELETE /api/pages/:page - Delete a page by page name
router.delete("/:page", async (req, res) => {
  try {
    const db = mongoose.connection.db;
    const collection = db.collection("pages");

    const deletedPage = await collection.findOneAndDelete({
      page: req.params.page,
    });

    if (!deletedPage) {
      return res.status(404).json({
        success: false,
        message: "Page not found",
      });
    }

    res.json({
      success: true,
      message: "Page deleted successfully",
      data: deletedPage,
    });
  } catch (error) {
    console.error("Error deleting page:", error);
    res.status(500).json({
      success: false,
      message: "Error deleting page",
      error: error.message,
    });
  }
});

router.post("/upload", upload.single("image"), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({
        success: false,
        message: "No file provided. Please upload an image, PDF, or video file.",
      });
    }

    const { pageId, fieldPath, sections } = req.body;
    const imageUrl = `uploads/${req.file.filename}`;

    // If pageId is provided, update the page in MongoDB
    if (pageId) {
      const db = mongoose.connection.db;
      const collection = db.collection("pages");

      // Check if page exists
      const existingPage = await collection.findOne({
        _id: new mongoose.Types.ObjectId(pageId),
      });

      if (!existingPage) {
        return res.status(404).json({
          success: false,
          message: "Page not found",
        });
      }

      // Prepare update data
      const updateData = {
        updatedAt: new Date(),
      };

      // Process all body fields for nested updates
      const dottedFields = {};
      Object.keys(req.body).forEach((key) => {
        if (key.includes(".")) {
          // This is a nested field - handle it specially
          if (key === fieldPath) {
            // This is the field we want to update with the image URL
            dottedFields[key] = imageUrl;
          } else {
            // Other nested fields - keep their values
            dottedFields[key] = req.body[key];
          }
        }
      });

      // If fieldPath is provided (e.g., "sections.aboutUs.0.image"), update that specific field
      if (fieldPath) {
        dottedFields[fieldPath] = imageUrl;
      }

      // If sections data is provided, parse and update it
      if (sections) {
        let parsedSections = sections;
        if (typeof sections === "string") {
          try {
            parsedSections = JSON.parse(sections);
          } catch (e) {
            return res.status(400).json({
              success: false,
              message: "Invalid sections JSON format",
              error: e.message,
            });
          }
        }

        // Replace placeholder images with the uploaded image URL
        if (parsedSections) {
          parsedSections = processImagesWithSections(parsedSections, [imageUrl]);
          updateData.sections = parsedSections;
        }
      }

      // Build the $set operation for nested updates
      const setOperation = {};

      // Add dotted fields (including the image URL)
      Object.keys(dottedFields).forEach((key) => {
        setOperation[key] = dottedFields[key];
      });

      // Handle nested field updates from regular body
      Object.keys(updateData).forEach((key) => {
        if (key.includes(".")) {
          setOperation[key] = updateData[key];
          delete updateData[key];
        }
      });

      // Add remaining fields to setOperation
      Object.keys(updateData).forEach((key) => {
        if (key !== "_id") {
          setOperation[key] = updateData[key];
        }
      });

      // Update the page in MongoDB
      const updatedPage = await collection.findOneAndUpdate(
        { _id: new mongoose.Types.ObjectId(pageId) },
        { $set: setOperation },
        { returnDocument: "after" }
      );

      if (updatedPage) {
        const fileInfo = {
          originalName: req.file.originalname,
          size: req.file.size,
          mimetype: req.file.mimetype,
          path: req.file.path,
          url: imageUrl,
        };

        // Use appropriate field name based on file type
        if (req.file.mimetype === "application/pdf") {
          fileInfo.pdf = req.file.filename;
        } else if (req.file.mimetype.startsWith("video/")) {
          fileInfo.video = req.file.filename;
        } else {
          fileInfo.filename = req.file.filename;
        }

        return res.json({
          success: true,
          message: "File uploaded and page updated successfully",
          file: fileInfo,
          updatedPage: updatedPage,
          timestamp: new Date().toISOString(),
        });
      }
    }

    // If no pageId provided, just return file information
    const fileInfo = {
      originalName: req.file.originalname,
      size: req.file.size,
      mimetype: req.file.mimetype,
      path: req.file.path,
      url: imageUrl,
    };

    // Use appropriate field name based on file type
    if (req.file.mimetype === "application/pdf") {
      fileInfo.pdf = req.file.filename;
    } else if (req.file.mimetype.startsWith("video/")) {
      fileInfo.video = req.file.filename;
    } else {
      fileInfo.filename = req.file.filename;
    }

    res.json({
      success: true,
      message: "File uploaded successfully",
      file: fileInfo,
      timestamp: new Date().toISOString(),
    });
  } catch (error) {
    console.error("Upload error:", error);
    res.status(500).json({
      success: false,
      message: "Internal server error during upload",
      error: error.message,
    });
  }
});

module.exports = router;
