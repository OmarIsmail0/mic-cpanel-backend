const express = require("express");
const router = express.Router();
const Page = require("../models/Page");
const { uploadSingle, uploadMultiple, handleUploadError } = require("../middleware/upload");

// GET /api/pages - Get all pages
router.get("/", async (req, res) => {
  try {
    const pages = await Page.find().sort({ createdAt: -1 });
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
router.get("/:id", async (req, res) => {
  try {
    const mongoose = require("mongoose");
    const db = mongoose.connection.db;
    const collection = db.collection("pages");

    const page = await collection.findOne({ _id: new mongoose.Types.ObjectId(req.params.id) });
    // const page = await collection.findOne({ page: req.params.page });
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
router.post("/", uploadMultiple, handleUploadError, async (req, res) => {
  try {
    const { page, sections } = req.body;

    // Process uploaded images
    let imageUrls = [];
    if (req.files && req.files.length > 0) {
      imageUrls = req.files.map((file) => `/uploads/${file.filename}`);
    }

    const newPage = new Page({
      page,
    });

    const savedPage = await newPage.save();

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

// PUT /api/pages/:id - Update a page
router.put("/:id", uploadMultiple, handleUploadError, async (req, res) => {
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

    // Process uploaded images
    let imageUrls = [];
    if (req.files && req.files.length > 0) {
      imageUrls = req.files.map((file) => `/uploads/${file.filename}`);
    }

    // Use request body directly as update data, but exclude _id field
    const { _id, ...updateData } = req.body;

    // Add timestamp for update
    updateData.updatedAt = new Date();

    const updatedPage = await collection.findOneAndUpdate(
      { _id: new mongoose.Types.ObjectId(req.params.id) },
      { $set: updateData },
      { returnDocument: "after" }
    );

    res.json({
      success: true,
      message: "Page updated successfully",
      data: updatedPage,
    });
  } catch (error) {
    console.error("Error updating page:", error);
    res.status(500).json({
      success: false,
      message: "Error updating page",
      error: error.message,
    });
  }
});

// DELETE /api/pages/:id - Delete a page
router.delete("/:id", async (req, res) => {
  try {
    const deletedPage = await Page.findByIdAndDelete(req.params.id);

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

module.exports = router;
