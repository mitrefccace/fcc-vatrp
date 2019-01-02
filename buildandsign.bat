nuget restore VATRP.sln
set vatrpVersion=1.0
set vatrpConfiguration="Release"
set cert="CARoot.pfx"
set PATH="C:\Program Files (x86)\WiX Toolset v3.10\bin";%PATH%
set PATH="C:\Program Files (x86)\Windows Kits\8.1\bin\x64";%PATH%
msbuild VATRP.sln /p:Configuration=%vatrpConfiguration%  /p:Targets="Build" /p:BuildInParallel=true /m
signtool sign  /f %cert%  /a ".\VATRP.App\bin/Release\ACE.exe"
signtool sign /f %cert% /a ".\VATRP.App\bin/Release\*.dll"
signtool sign /f %cert% /a ".\VATRP.App\bin/Release\lib\mediastreamer\plugins\*.dll"
Candle -dVATRP.Version=%vatrpVersion% -dConfiguration=%vatrpConfiguration% -dOutDir=.\Setup\Out -dPlatform=x86 -dTargetDir=.\Setup\Out -dTargetExt=.msi -out .\Setup\obj\ -arch x86 -ext WixNetFxExtension.dll -ext WixUtilExtension.dll -ext WixUIExtension.dll .\Setup\ACE-Setup.wxs
Light -out .\Setup\Out\ACE_Setup_%vatrpVersion%.msi -pdbout .\Setup\obj\ACE_Setup.wixpdb -cultures:null -ext WixNetFxExtension.dll -ext WixUtilExtension.dll -ext WixUIExtension.dll -contentsfile .\Setup\obj\ACE-Setup.wixproj.BindContentsFileListnull.txt -outputsfile .\Setup\obj\ACE-Setup.wixproj.BindOutputsFileListnull.txt -builtoutputsfile .\Setup\obj\ACE-Setup.wixproj.BindBuiltOutputsFileListnull.txt  .\Setup\obj\ACE-Setup.wixobj
rd /S /Q .\Setup\obj
signtool sign /f %cert% /a ".\Setup\Out\ACE_Setup_%vatrpVersion%.msi"
pause