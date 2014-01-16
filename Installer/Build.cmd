
del "..\MangaCrawler\bin\Release\MangaCrawler.exe" /q
del "*.exe" /q

call "C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools\vsvars32.bat"
devenv "..\MangaCrawler.sln" /rebuild "Release" /ProjectConfig "Any CPU"
"C:\Program Files (x86)\Inno Setup 5\ISCC.exe" Installer.iss

pause