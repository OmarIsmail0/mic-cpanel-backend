const jwt = require("jsonwebtoken");

const auth = (req, res, next) => {
  try {
    const token = req.header("Authorization")?.replace("Bearer ", "");

    if (!token) {
      return res.status(401).json({
        message: "No token provided, access denied",
      });
    }

    const decoded = jwt.verify(token, process.env.JWT_SECRET);
    req.adminId = decoded.adminId;
    req.username = decoded.username;
    next();
  } catch (error) {
    console.error("Auth middleware error:", error);
    res.status(401).json({
      message: "Token is not valid",
    });
  }
};

module.exports = auth;
