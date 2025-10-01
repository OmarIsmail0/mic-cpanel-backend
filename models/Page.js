const mongoose = require("mongoose");

// Schema for multilingual text
const multilingualTextSchema = {
  id: {
    type: String,
    required: true,
  },
  en: {
    type: String,
    required: true,
  },
  ar: {
    type: String,
    required: true,
  },
};

// Schema for pointer arrays (nested multilingual text)
const pointerSchema = {
  type: [multilingualTextSchema],
  default: [],
};

const pageSchema = new mongoose.Schema(
  {
    page: {
      type: String,
      required: true,
    },
    sections: {
      images: {
        type: [String],
        default: [],
      },
      titles: {
        type: [multilingualTextSchema],
        default: [],
      },
      descriptions: {
        type: [multilingualTextSchema],
        default: [],
      },
      pointers: {
        type: [pointerSchema],
        default: [],
      },
    },
  },
  {
    timestamps: true,
    collection: "pages", // Explicitly set collection name
  }
);

// Index for better query performance
pageSchema.index({ page: 1 });

const Page = mongoose.model("Page", pageSchema);

module.exports = Page;
