const mongoose = require("mongoose");

const formSchema = new mongoose.Schema(
  {
    formName: {
      type: String,
      required: true,
    },
    formData: {
      type: Object,
      required: true,
    },
  },
  {
    timestamps: true,
    collection: "forms", // Explicitly set collection name
  }
);

// Index for better query performance
formSchema.index({ formName: 1 });

const Form = mongoose.model("Form", formSchema);

module.exports = Form;
