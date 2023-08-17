# MediaWizForums #
Simple Forum add on for Umbraco ≥ 10. 

## 10.6.1 ##
1. Fixed issue with password reset
2. Fixed issue with index and searching
3. Updated static strings in viewcomponents to use dictionary strings
4. Added viewcomponent for active topic filter
5. FIxed issue with indexing of updated field

# IMPORTANT! 10.6.0 update #
Some code refactoring and a change to the way the migration works, A problem was discovered where the install would fail
if your website already contained any document types using the same aliases as the forum package. 

doctypes created that may cause conflicts "login", "members", "profile", "register", "reset", "verify"

If you encounter this issue it is possible to add a setting to the "MediaWizOptions"

```
  "MediaWizOptions": {
    "ForumDoctypes": "prefix",
    ...
  }
```
Adding this value will force the install to load a different package.xml in the migration and create the document types using the prefix "forum" instead, this should avoid any conflicts

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

