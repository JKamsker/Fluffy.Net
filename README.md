# Fluffy .NET

Fluffy .NET is an easy-to-use Network library wich contains many useful features.

#BUILDING

- Framework Version > .NET 4.7
-- Compiler Flag: NET47; NET46 ; NET45; NET40
-- Unused Flags: NET80

# Replace for testing other Target Frameworks:
	<DefineConstants>TRACE;DEBUG;NET47</DefineConstants>
	<TargetFrameworkVersion>.*?</TargetFrameworkVersion>

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
