// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Resources;

// NOTE: We're not actually using this project to generate a .NET class library
// so these settings are more or less meaningless.  We've just got this project
// as there's no option for a "JavaScript Class Library" for Win8 projects.

[assembly: SuppressMessage(
    "Microsoft.Design",
    "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "Not an actual class library.")]
[assembly: NeutralResourcesLanguageAttribute("en-US")]
