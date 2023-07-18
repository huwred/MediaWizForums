# MediaWizForums #
Simple Forum add on for Umbraco ≥ 10. 
## New version 10 nuget package released ##
1. Removed partial view files and replaced with Viewcomponents
2. Removed dependency on platform specific System.Drawing for Captcha control
3. Template views maintened in RCL rather than in package.zip

Tested in v10 and v11 of Umbraco

### 10.4.4 ###
Umbraco Security Patch

### 10.4.3 ###
Fixed issue with ForgotPassword code

### 10.4.2 ###
Fixed issue with tinyMCE image uploads
Fixed issue with sorting, stickiness values

Added 'My Files' section to members profile page
Added Config section to appsettings.json
```
  "MediaWizOptions": {
    "MaxFileSize": 8,  - Maximum file size in MB
    "AllowedFiles": [ ".gif", ".jpg", ".png", ".svg", ".webp" ], - Allowed image file extensions
    "UniqueFilenames": true - if true uses random guid for filename, if false uses name of uploaded file
  }
```

### 10.4.1 ###
Added 'mark as answered' to posts
Update to Custom ForumIndex fields + Rebuild on publish post
Changed DisplayPost Razor function into ViewComponent
Fixed issue with tinyMCE initialisation in modal popups

