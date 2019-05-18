# Fluffy .NET  
Available at [![NuGet version (Fluffy.Net)](https://img.shields.io/nuget/v/Fluffy.Net.svg?style=flat-square)](https://www.nuget.org/packages/Fluffy.Net/)


| Package  |  Build Status  | Nuget |
|---|---|---|
| Fluffy | [![Build Status](https://dev.azure.com/jonaskamsker/Fluffy.Net/_apis/build/status/J-kit.Fluffy.Net?branchName=master)](https://dev.azure.com/jonaskamsker/Fluffy.Net/_build/latest?definitionId=2&branchName=master)  | [![NuGet version (Fluffy)](https://img.shields.io/nuget/v/Fluffy.svg?style=flat-square)](https://www.nuget.org/packages/Fluffy/)  |
| Fluffy.IO | [![Build Status](https://dev.azure.com/jonaskamsker/Fluffy.Net/_apis/build/status/J-kit.Fluffy.Net?branchName=master)](https://dev.azure.com/jonaskamsker/Fluffy.Net/_build/latest?definitionId=2&branchName=master)  | [![NuGet version (Fluffy.IO)](https://img.shields.io/nuget/v/Fluffy.IO.svg?style=flat-square)](https://www.nuget.org/packages/Fluffy.IO/)  |
| Fluffy.Net | [![Build Status](https://dev.azure.com/jonaskamsker/Fluffy.Net/_apis/build/status/J-kit.Fluffy.Net?branchName=master)](https://dev.azure.com/jonaskamsker/Fluffy.Net/_build/latest?definitionId=2&branchName=master)  | [![NuGet version (Fluffy.Net)](https://img.shields.io/nuget/v/Fluffy.Net.svg?style=flat-square)](https://www.nuget.org/packages/Fluffy.Net/)  |
| Fluffy.Unsafe | [![Build Status](https://dev.azure.com/jonaskamsker/Fluffy.Net/_apis/build/status/J-kit.Fluffy.Net?branchName=master)](https://dev.azure.com/jonaskamsker/Fluffy.Net/_build/latest?definitionId=2&branchName=master)  | [![NuGet version (Fluffy.Unsafe)](https://img.shields.io/nuget/v/Fluffy.Unsafe.svg?style=flat-square)](https://www.nuget.org/packages/Fluffy.Unsafe/)  |


Fluffy .NET is an easy-to-use Network library wich contains many useful features.






# BUILDING

- Framework Version >= .NET 4.7
-- Compiler Flag: NET47; NET46 ; NET45; NET40
-- Unused Flags: NET80

## Replace for testing other Target Frameworks:
	<DefineConstants>TRACE;DEBUG;NET47</DefineConstants>
	<TargetFrameworkVersion>.*?</TargetFrameworkVersion>

## Building with MSBUILD:
 - Example: MSBuild.exe Fluffy.sln /t:Rebuild /p:Configuration=Release /p:TargetFrameworkVersion='v4.7' /p:DefineConstants="NET47"

# Todo
- Raw-Stream sending API
- Sending and awaiting Response (+async/await support)
- RMI Interface

# Points for Improvements
This point doesnt mean much. Its just a little notice for the core developer
- Fluffy.Net.Async.SharedOutputQueueWorker: Making the ThreadWorker check the queue more often before sleeping
- [DONE]Fluffy.Fluent.TypeSwitch: Working with Dictionaries in order to boost performance 
- Fluffy.Net.Async.AsyncSender: Fill buffer

### Documentation created with <3 and stackedit.io
