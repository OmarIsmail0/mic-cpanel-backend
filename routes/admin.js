const express = require("express");
const auth = require("../middleware/auth");
const fs = require("fs").promises;
const path = require("path");

const router = express.Router();

// @route   GET /api/admin/dashboard
// @desc    Get dashboard data
// @access  Private
router.get("/dashboard", auth, async (req, res) => {
  try {
    // Get basic system information
    const systemInfo = {
      nodeVersion: process.version,
      platform: process.platform,
      uptime: process.uptime(),
      memory: process.memoryUsage(),
      timestamp: new Date().toISOString(),
    };

    // Check if MIC website path exists
    let micWebsiteStatus = "not_configured";
    let micWebsiteFiles = [];

    if (process.env.MIC_WEBSITE_PATH) {
      try {
        const files = await fs.readdir(process.env.MIC_WEBSITE_PATH);
        micWebsiteStatus = "accessible";
        micWebsiteFiles = files.slice(0, 10); // Show first 10 files
      } catch (error) {
        micWebsiteStatus = "inaccessible";
      }
    }

    res.json({
      systemInfo,
      micWebsite: {
        path: process.env.MIC_WEBSITE_PATH,
        status: micWebsiteStatus,
        files: micWebsiteFiles,
      },
      database: {
        status: "connected", // Assuming MongoDB is connected if this route is reached
        collections: [], // You can add collection info here
      },
    });
  } catch (error) {
    console.error("Dashboard error:", error);
    res.status(500).json({
      message: "Server error while fetching dashboard data",
    });
  }
});

// @route   GET /api/admin/website/files
// @desc    Get MIC website files
// @access  Private
router.get("/website/files", auth, async (req, res) => {
  try {
    if (!process.env.MIC_WEBSITE_PATH) {
      return res.status(400).json({
        message: "MIC website path not configured",
      });
    }

    const { directory = "" } = req.query;
    const fullPath = path.join(process.env.MIC_WEBSITE_PATH, directory);

    // Security check - ensure path is within MIC_WEBSITE_PATH
    if (!fullPath.startsWith(process.env.MIC_WEBSITE_PATH)) {
      return res.status(403).json({
        message: "Access denied",
      });
    }

    const files = await fs.readdir(fullPath, { withFileTypes: true });
    const fileList = files.map((file) => ({
      name: file.name,
      type: file.isDirectory() ? "directory" : "file",
      path: path.join(directory, file.name),
    }));

    res.json({
      files: fileList,
      currentPath: directory,
      parentPath: path.dirname(directory),
    });
  } catch (error) {
    console.error("File listing error:", error);
    res.status(500).json({
      message: "Error reading directory",
    });
  }
});

// @route   GET /api/admin/website/file
// @desc    Get file content
// @access  Private
router.get("/website/file", auth, async (req, res) => {
  try {
    const { filePath } = req.query;

    if (!filePath) {
      return res.status(400).json({
        message: "File path is required",
      });
    }

    if (!process.env.MIC_WEBSITE_PATH) {
      return res.status(400).json({
        message: "MIC website path not configured",
      });
    }

    const fullPath = path.join(process.env.MIC_WEBSITE_PATH, filePath);

    // Security check
    if (!fullPath.startsWith(process.env.MIC_WEBSITE_PATH)) {
      return res.status(403).json({
        message: "Access denied",
      });
    }

    const content = await fs.readFile(fullPath, "utf8");
    const stats = await fs.stat(fullPath);

    res.json({
      content,
      stats: {
        size: stats.size,
        modified: stats.mtime,
        isFile: stats.isFile(),
        isDirectory: stats.isDirectory(),
      },
    });
  } catch (error) {
    console.error("File reading error:", error);
    res.status(500).json({
      message: "Error reading file",
    });
  }
});

// @route   PUT /api/admin/website/file
// @desc    Update file content
// @access  Private
router.put("/website/file", auth, async (req, res) => {
  try {
    const { filePath, content } = req.body;

    if (!filePath || content === undefined) {
      return res.status(400).json({
        message: "File path and content are required",
      });
    }

    if (!process.env.MIC_WEBSITE_PATH) {
      return res.status(400).json({
        message: "MIC website path not configured",
      });
    }

    const fullPath = path.join(process.env.MIC_WEBSITE_PATH, filePath);

    // Security check
    if (!fullPath.startsWith(process.env.MIC_WEBSITE_PATH)) {
      return res.status(403).json({
        message: "Access denied",
      });
    }

    await fs.writeFile(fullPath, content, "utf8");

    res.json({
      message: "File updated successfully",
    });
  } catch (error) {
    console.error("File update error:", error);
    res.status(500).json({
      message: "Error updating file",
    });
  }
});

module.exports = router;
