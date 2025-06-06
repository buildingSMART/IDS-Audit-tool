﻿using FluentAssertions;
using System.Diagnostics;
using Xunit;

namespace idsTool.tests
{
    public class VersionTests
    {
		/// <summary>
		/// This makes sure that the <see cref="IdsLib.LibraryInformation.AssemblyVersion"/> is kept up to date.
		/// </summary>
		[Fact]
        public void HardCodedVersion_Matches()
        {
            var assembly = typeof(IdsLib.Audit).Assembly;
            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location);
            // <== fix IdsLib.LibraryInformation.AssemblyVersion
            IdsLib.LibraryInformation.AssemblyVersion.Should().Be(fileVersion.FileVersion); 
        }

	}
}
