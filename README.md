[![Build Status](https://travis-ci.org/MinhThienDX/save-lockscreen-image.svg?branch=master)](https://travis-ci.org/MinhThienDX/save-lockscreen-image)

# save-lockscreen-image
A dead simple console app to save Windows 10's lockscreen images  
Just config your setting in App.config and run the exe file

### Settings
#### 1. Save folder
Setting your path in `destFolder` (E:\Pictures)
#### 2. Filter
   - `minFileSizeInByte` : By minimum filesize (in bytes, default is 102400, so files > 100 KB is processed)
   - `minImageWidth` : By minimum width (in pixels, default is 1900 pixels so only Full HD images is processed)
#### 3. Delete duplicated image  
Sometime, filename is changed for unknown reason and may lead to 2 duplicated images with different names  
Therefore, `deleteDuplicate` setting will help you with that  
Only files with **matching size and MD5 hash** will be deleted  
Deleted files will go to Recycle Bin

#### TODO
- [ ] Check `minFileSizeInByte` with value 0
- [ ] Check `minImageWidth` with value 0
- [ ] Publish this somewhere for people to use
