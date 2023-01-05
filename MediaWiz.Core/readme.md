# MediaWizForums #
Simple Forum add on for Umbraco ≥ 10. 
## New version 10 nuget package released ##
1. Removed partial view files and replaced with Viewcomponents
2. Removed dependency on platform specific System.Drawing for Captcha control
3. Template views maintened in RCL rather than in package.zip

Tested in v10 and v11 of Umbraco

### 10.4.1 ###
Added 'mark as answered' to posts
Update to Custom ForumIndex fields + Rebuild on publish post
Chaged DisplayPost Razor function into ViewComponent
Fixed issue with tinyMCE initialisation in modal popup