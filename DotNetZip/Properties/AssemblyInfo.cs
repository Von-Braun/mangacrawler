using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Ionic's Zip Library")]


#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyDescription("a library for handling zip archives. http://www.codeplex.com/DotNetZip (Flavor=Debug)")]
#else
[assembly: AssemblyConfiguration("Retail")]
[assembly: AssemblyDescription("a library for handling zip archives. http://www.codeplex.com/DotNetZip (Flavor=Retail)")]
#endif


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("dfd2b1f6-e3be-43d1-9b43-11aae1e901d8")]

[assembly:System.CLSCompliant(true)]

[assembly: AssemblyCompany("Dino Chiesa")]
[assembly: AssemblyProduct("DotNetZip Library")]
[assembly: AssemblyCopyright("Copyright © Dino Chiesa 2006 - 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


[assembly: AssemblyVersion("1.9.1.8")]

[assembly: AssemblyFileVersion("1.9.1.8")]
[assembly: AllowPartiallyTrustedCallers]



