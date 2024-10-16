﻿/*
	Copyright © Bryan Apellanes 2024  
*/
using System.Reflection;
using System.Runtime.CompilerServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyCopyright("Copyright © Bryan Apellanes 2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]



// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

[assembly: InternalsVisibleTo("bam.data")]
[assembly: InternalsVisibleTo("bam.data.schema")]
[assembly: InternalsVisibleTo("bam.data.repositories")]
[assembly: InternalsVisibleTo("bam.data.dynamic")]
[assembly: InternalsVisibleTo("bam.data.backup")]
[assembly: InternalsVisibleTo("bam.caching")]
[assembly: InternalsVisibleTo("bam.commandline")]
[assembly: InternalsVisibleTo("bam.console")]
