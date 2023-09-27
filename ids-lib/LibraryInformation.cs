using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdsLib
{
	/// <summary>
	/// General information on the assembly without reflection.
	/// 
	/// This is useful for environments that do not allow to load information from the DLL dynamically (e.g. Blazor).
	/// </summary>
	public class LibraryInformation
	{
		/// <summary>
		/// Static field with the short commit hash at compilation time
		/// </summary>
		public static string Commit => ThisAssembly.Git.Commit + (ThisAssembly.Git.IsDirty ? " dirty" : "");
		/// <summary>
		/// Static field with the full SHA hash at compilation time
		/// </summary>
		public static string Sha => ThisAssembly.Git.Sha + (ThisAssembly.Git.IsDirty ? " dirty" : "");
		/// <summary>
		/// Static field with the full datetime string of the commit.
		/// </summary>
		public static string CommitDate  => ThisAssembly.Git.CommitDate;
		/// <summary>
		/// Static field with hardcoded DLL version number. 
		/// </summary>
		public static string AssemblyVersion => "1.0.42";
	}
}
