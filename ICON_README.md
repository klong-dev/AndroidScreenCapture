# NuGet Package Icon

Due to file size constraints, please add a 128x128 PNG icon file named `icon.png` to the root directory.

Suggested icon design:
- Android robot with screenshot/camera symbol
- Colors: Android green (#3DDC84) with modern design
- 128x128 pixels minimum
- PNG format
- Transparent background recommended

You can create the icon using:
- Canva (free online design tool)
- GIMP (free image editor)
- Adobe Illustrator/Photoshop
- Online icon generators

Once you have the icon, add it to the .csproj file:
```xml
<PackageIcon>icon.png</PackageIcon>
```

And include it in the ItemGroup:
```xml
<None Include="icon.png" Pack="true" PackagePath="\" />
```
