[![Build Status](https://travis-ci.org/MinhThienDX/SaveLockscreenImage.svg?branch=master)](https://travis-ci.org/MinhThienDX/SaveLockscreenImage)
[![Build status](https://ci.appveyor.com/api/projects/status/bo8xopcrp6h4jjo8/branch/master?svg=true)](https://ci.appveyor.com/project/MinhThienDX/savelockscreenimage/branch/master)
[![CircleCI](https://circleci.com/gh/MinhThienDX/SaveLockscreenImage/tree/master.svg?style=svg)](https://circleci.com/gh/MinhThienDX/SaveLockscreenImage/tree/master)

# SaveLockscreenImage
A dead simple console app to save Windows 10's lockscreen images  
Just config your setting in App.config and run the exe file

### Settings
#### 1. Save folder
Setting your path in `destFolder` (E:\Pictures)
#### 2. Filter
   - `minFileSizeInByte` : By minimum filesize (in bytes, default is 102400, so only files > 100 KB is processed)
   - `minImageWidth` : By minimum width (in pixels, default is 1900 pixels so only Full HD images is processed)
#### 3. Delete duplicated image  
Sometime, filename is changed for unknown reason and may lead to 2 duplicated images with different names  
Therefore, `deleteDuplicate` setting will help you with that  
Only files with **matching size and MD5 hash** will be deleted  
Deleted files will go to Recycle Bin

#### TODO
- [x] Check `minFileSizeInByte` with value 0 (Add image file check in revision [18685ca](https://github.com/MinhThienDX/SaveLockscreenImage/commit/18685cac43a22b0b2c796b9444f501e18b4b85a0))
- [x] Check `minImageWidth` with value 0 (No error, just images with wrong file extension)
- [x] Publish to GitHub
- [x] Add Travis CI
- [x] Add AppVeyor CI
- [x] Add CircleCI (lack auto testing)
- [x] Add some testing
- [ ] Complete testing
- [ ] Add website