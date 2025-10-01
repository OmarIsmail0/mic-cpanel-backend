const express = require("express");
const router = express.Router();
const Form = require("../models/Form");
const { uploadSingle, uploadMultiple, handleUploadError } = require("../middleware/upload");

// GET /api/forms - Get all forms
router.get("/", async (req, res) => {
  try {
    const forms = await Form.find().sort({ createdAt: -1 });
    res.status(200).json({
      success: true,
      data: forms,
      count: forms.length,
    });
  } catch (error) {
    console.error("Error fetching forms:", error);
    res.status(500).json({
      success: false,
      message: "Error fetching forms",
      error: error.message,
    });
  }
});

// POST /api/forms - Create a new form
router.post("/", async (req, res) => {
  try {
    const { formName, formData } = req.body;
    const newForm = new Form({ formName, formData });
    const savedForm = await newForm.save();
    res.status(201).json({
      success: true,
      message: "Form created successfully",
      data: savedForm,
    });
  } catch (error) {
    console.error("Error creating form:", error);
    res.status(500).json({
      success: false,
      message: "Error creating form",
      error: error.message,
    });
  }
});

// DELETE /api/forms/:id - Delete a form
router.delete("/:id", async (req, res) => {
  try {
    const { id } = req.params;
    const deletedForm = await Form.findByIdAndDelete(id);

    if (!deletedForm) {
      return res.status(404).json({
        success: false,
        message: "Form not found",
      });
    }

    res.json({
      success: true,
      message: "Form deleted successfully",
      data: deletedForm,
    });
  } catch (error) {
    console.error("Error deleting form:", error);
    res.status(500).json({
      success: false,
      message: "Error deleting form",
      error: error.message,
    });
  }
});

module.exports = router;
