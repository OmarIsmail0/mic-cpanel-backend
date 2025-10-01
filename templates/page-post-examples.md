# Pages API - POST Endpoint Template

## Endpoint

```
POST http://localhost:5000/api/pages
Content-Type: multipart/form-data
```

## Form Data Fields

### Required Fields

- `page` (string): Display name of the page
- `sections` (JSON string): Page content sections

### Optional Fields

- `images` (files): Image files to upload (max 10, 5MB each)

## JSON Template for `sections` Field

```json
{
  "page": "Home Page",
  "sections": {
    "images": ["/uploads/hero-image.jpg", "/uploads/feature-image.png"],
    "titles": [
      {
        "id": "main-title",
        "en": "Welcome to MIC",
        "ar": "مرحبا بكم في MIC"
      },
      {
        "id": "subtitle",
        "en": "Leading Technology Solutions",
        "ar": "حلول تكنولوجية رائدة"
      }
    ],
    "descriptions": [
      {
        "id": "intro-desc",
        "en": "We provide innovative technology solutions for businesses worldwide. Our team of experts delivers cutting-edge products and services.",
        "ar": "نقدم حلول تكنولوجية مبتكرة للشركات في جميع أنحاء العالم. فريق الخبراء لدينا يقدم منتجات وخدمات متطورة."
      }
    ],
    "pointers": [
      [
        {
          "id": "feature-1",
          "en": "Innovation First",
          "ar": "الابتكار أولاً"
        },
        {
          "id": "feature-2",
          "en": "Customer Focused",
          "ar": "التركيز على العملاء"
        },
        {
          "id": "feature-3",
          "en": "Quality Assured",
          "ar": "الجودة مضمونة"
        }
      ],
      [
        {
          "id": "benefit-1",
          "en": "24/7 Support",
          "ar": "دعم على مدار الساعة"
        },
        {
          "id": "benefit-2",
          "en": "Fast Delivery",
          "ar": "تسليم سريع"
        }
      ]
    ]
  }
}
```

## Example cURL Command

```bash
curl -X POST http://localhost:5000/api/pages \
  -F "page=About Us" \
  -F 'sections={"images":["/uploads/about-hero.jpg"],"titles":[{"id":"title-1","en":"About MIC","ar":"حول MIC"}],"descriptions":[{"id":"desc-1","en":"We are a leading tech company","ar":"نحن شركة تكنولوجيا رائدة"}],"pointers":[[{"id":"p1","en":"Innovation","ar":"ابتكار"}]]}' \
  -F "images=@/path/to/image1.jpg" \
  -F "images=@/path/to/image2.png"
```

## Example JavaScript (Fetch API)

```javascript
const formData = new FormData();
formData.append("page", "Contact Us");
formData.append(
  "sections",
  JSON.stringify({
    images: [],
    titles: [
      {
        id: "contact-title",
        en: "Get In Touch",
        ar: "تواصل معنا",
      },
    ],
    descriptions: [
      {
        id: "contact-desc",
        en: "We'd love to hear from you",
        ar: "نحب أن نسمع منك",
      },
    ],
    pointers: [],
  })
);

// Add image files
const imageFiles = document.getElementById("imageInput").files;
for (let file of imageFiles) {
  formData.append("images", file);
}

fetch("http://localhost:5000/api/pages", {
  method: "POST",
  body: formData,
})
  .then((response) => response.json())
  .then((data) => console.log(data));
```

## Response Format

```json
{
  "success": true,
  "message": "Page created successfully",
  "data": {
    "_id": "64f8a1b2c3d4e5f6a7b8c9d0",
    "page": "Home Page",
    "sections": {
      "images": ["/uploads/image-1234567890-123456789.jpg"],
      "titles": [...],
      "descriptions": [...],
      "pointers": [...]
    },
    "createdAt": "2023-09-10T12:00:00.000Z",
    "updatedAt": "2023-09-10T12:00:00.000Z"
  }
}
```

## Notes

- **MongoDB \_id**: Pages use MongoDB's auto-generated `_id` field for identification
- **Images**: Upload files using the `images` field (multiple files allowed)
- **Image URLs**: Will be automatically generated as `/uploads/filename.ext`
- **Multilingual**: All text fields support both English (`en`) and Arabic (`ar`)
- **Pointers**: Array of arrays - each inner array represents a group of related points
- **Validation**: File size limit 5MB, max 10 images per request
- **File Types**: Only image files allowed (jpg, jpeg, png, gif, webp)
