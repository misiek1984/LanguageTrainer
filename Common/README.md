# Common
*Common* solution is a collection of libraries, utilities, algorithms, helpers etc. (written in C#) that are used in my other projects. Here is a short description of them:

1. *MK.Compression* - A library that allows one to pack/unpack data. I implemented it before [System.IO.Compression.ZipFile][1] class was added to .NET framework.
2. *MK.Data* - This library contains a basic classes and interfaces related to data persistence.
3. *MK.Data.RelationalDB* - A lightweight wrapper over ADO.NET.
4. *MK.Data.Xml*- A library that makes writing/reading XML data easier.
5. *MK.Dropbox* - A library based on [DropNet][2] project that facilitates communication with Dropbox.
6. *MK.Live* - A library based on [Microsoft.Live][3] project facilitates communication with SkyDrive.
7. *MK.Logging* - A logging library that is currently based on [Enterprise Library Logging Application Block][4].
8. *MK.MyMath* - A library that contains math algoritihms and data structures.
9. *MK.Scripting* - A scripting engine for C# based on [Mono.CSharp][5] project.
10. *MK.Settings* - A library that makes managing settings easily. Firstly you should use ```SettingsProperty``` attribute to to mark properties that should be persisted. Then you should use ```SettingsProvider``` either to extract/inject values from/into a given object.
11. *MK.UI.WPF* - Various WPF controls, behaviours and converters. A library also contains MVVM helpers.
12. *MK.Utilities* - Various things that are too small for individual libraries. For example: extension methods, an encryption helper, an object dumper...
13. *WPFLocalizeExtension* - This project is based on the old version of [WPFLocalisationExtension][5] project that was available on codeplex.

[1]: https://msdn.microsoft.com/en-us//library/system.io.compression.zipfile(v=vs.110).aspx
[2]: https://www.nuget.org/packages/DropNet
[3]: https://www.nuget.org/packages/LiveSDK
[4]: https://www.nuget.org/packages/EnterpriseLibrary.Logging/
[5]: https://www.nuget.org/packages/Mono.CSharp
[6]: http://wpflocalizeextension.codeplex.com/
