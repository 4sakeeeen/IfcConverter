1. Install dependencies\windowsdesktop-runtime-7.0.10-win-x64.exe
2. Register assembly Regsvr32.exe .\IngrDataRead.dll

License regedit:
	PATH: Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Intergraph\Pdlice_etc\
	NAME: pdlice_key_S
	TYPE: REG_BINARY


Default P3D classes to S3D classes mapping:
	<default_s3d_dir> - C:\Windows\Program Files (x86)\Smart3D
	FILE: <default_s3d_dir>\3DRefData\SharedContent\Xml\SP3D_FILES\SP3DPublishMap.xml
	DEFINITION: DefUID="MapClassToClass", WHERE UID1 - S3D class, UID2 - P3D Class

How to know S3D class (occurence) of instance in model:
(Example of piping feature):
	1. Ctrl + Shift + R
	2. Interfaces=True, Apply
	3. Goto "IJRtePathFeat" -> "PathGeneratedParts" -> "IJRtePathGenPart"