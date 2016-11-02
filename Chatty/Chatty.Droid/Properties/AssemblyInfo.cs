using System.Reflection;
using System.Runtime.CompilerServices;
using Android.App;

// Information about this assembly is defined by the following attributes.
// Change them to the values specific to your project.

[assembly: AssemblyTitle("AdMaiora Chatty Droid")]
[assembly: AssemblyDescription("AdMaiora Chatty Droid")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Ad Maiora Studio")]
[assembly: AssemblyProduct("AdMaiora Chatty Droid")]
[assembly: AssemblyCopyright("Copyright Ad Maiora Studio ©  2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// The assembly version has the format "{Major}.{Minor}.{Build}.{Revision}".
// The form "{Major}.{Minor}.*" will automatically update the build and revision,
// and "{Major}.{Minor}.{Build}.*" will update just the revision.

[assembly: AssemblyVersion ("1.0.0")]

// The following attributes are used to specify the signing key for the assembly,
// if desired. See the Mono documentation for more information about signing.

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]

// NOTE: Facebook SDK rquires that the 'Value' point to a string resource
//       in your values/ folder (eg: strings.xml file).
//       It will not allow you to use the app_id value directly here!
[assembly: MetaData("com.facebook.sdk.ApplicationId", Value = "@string/fb_app_id")]


