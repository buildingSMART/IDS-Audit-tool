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
	public static class LibraryInformation
	{
		/// <summary>
		/// Static field with the short commit hash at compilation time
		/// </summary>
		public static string Commit => ThisAssembly.Git.Commit;
		/// <summary>
		/// Static field with the full SHA hash at compilation time
		/// </summary>
		public static string Sha => ThisAssembly.Git.Sha;
		/// <summary>
		/// Static field with informing of possible dirty repository on compilation. When compiled remotely it does not necessarily mean that the code differs from the commit.
		/// </summary>
		public static bool Isdirty => ThisAssembly.Git.IsDirty;
		/// <summary>
		/// Static field with the full datetime string of the commit.
		/// </summary>
		public static string CommitDate  => ThisAssembly.Git.CommitDate;
		/// <summary>
		/// Static field with hardcoded DLL version number. 
		/// </summary>
		public static string AssemblyVersion => "1.0.58";
	}
}
