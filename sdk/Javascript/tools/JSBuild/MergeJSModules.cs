// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Microsoft.WindowsAzure.Mobile.JSBuild
{
    /// <summary>
    /// MSBuild task to merge JavaScript modules into a library that can be
    /// included like a regular script.  This gives us the engineering benefits
    /// of modules but integrates cleanly with any JavaScript platform that
    /// doesn't support modules natively (like Windows 8).
    /// 
    /// The modules to merge are defined by a .library file and follow the
    /// CommonJS Module Specification (defined in just a couple sentences at
    /// http://www.commonjs.org/specs/modules/1.0/) with a few small
    /// exceptions (only top level module names are supported and there's an
    /// additional free variable 'global' that refers to the top level scope).
    /// 
    /// You can also specify any of the modules as an entry point to the
    /// library and conditionally include modules in specialized versions of
    /// the library (useful for unit testing internals, etc.).
    /// </summary>
    /// <remarks>
    /// Library definitions have the following format:
    /// 
    ///    <Library
    ///      Path="{library}.js"
    ///      {Condition}Path="{library}.{condition}.js"
    ///      Copyright="{Optional Copyright Message}">
    ///         <Module Path="{first}.js" />
    ///         <Module Path="{second}.js" EntryPoint="true" />
    ///         <Module Path="{conditional}.js" Condition="{Condition}" />
    ///    </Library>
    ///  
    ///  The different options are explained in detail in the code below.
    /// </remarks>
    public class MergeJSModules : Task
    {
        /// <summary>
        /// Gets or sets paths to files containing the library definitions.
        /// </summary>
        [Required]
        public ITaskItem[] LibraryDefinitions { get; set; }

        /// <summary>
        /// Create library definitions.
        /// </summary>
        /// <returns>
        /// A value indicating whether the task completed successfully.
        /// </returns>
        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "Microsoft.Build.Utilities.TaskLoggingHelper.LogError(System.String,System.String,System.String,System.String,System.Int32,System.Int32,System.Int32,System.Int32,System.String,System.Object[])",
            Justification = "Literal error messages are acceptable for an internal build task.S")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "If anything does wrong with the copy, we want to report an error.  This is acceptable for an internal build task.")]
        public override bool Execute()
        {
            bool succeeded = true;
            foreach (ITaskItem definition in this.LibraryDefinitions)
            {
                try
                {
                    MergeModulesIntoLibrary(definition.ItemSpec);
                }
                catch (Exception ex)
                {
                    // Log an error pointing at the actual 
                    Log.LogError(
                        "MergeJSModules",
                        "MergeJSModules",
                        null,
                        definition.ItemSpec,
                        0,
                        0,
                        0,
                        0,
                        "Error merging JavaScript modules in {0}: {1}",
                        definition.ItemSpec,
                        ex);
                    succeeded = false;
                }
            }
            return succeeded;
        }

        /// <summary>
        /// Ensure the condition is satisfied or raise an error.
        /// </summary>
        /// <param name="condition">The condition to verify.</param>
        /// <param name="format">Error message format.</param>
        /// <param name="arguments">Error message arguments.</param>
        private static void Ensure(bool condition, string format, params object[] arguments)
        {
            if (!condition)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, format, arguments));
            }
        }

        /// <summary>
        /// Gets the value of an attribute or returns null if not found.
        /// </summary>
        /// <param name="element">
        /// The element to check for the attribute.
        /// </param>
        /// <param name="attribute">The attribute to check for.</param>
        /// <returns>The value of the attribute or null if not found.</returns>
        private static string GetAttribute(XElement element, string attribute)
        {
            Debug.Assert(element != null, "element should not be null.");
            Debug.Assert(!string.IsNullOrEmpty(attribute), "attribute should not be null or empty!");

            XAttribute attr = element.Attribute(attribute);
            return attr != null ?
                attr.Value :
                null;
        }

        /// <summary>
        /// Expand a path containing glob-style wildcards into the physical
        /// paths it represents.  Note that wildcards are only allowed in the
        /// file name - not the directory name.
        /// </summary>
        /// <param name="pathPattern">
        /// The path pattern (or just a regular path).
        /// </param>
        /// <returns>The corresponding physical paths.</returns>
        private static IEnumerable<string> ExpandPath(string pathPattern)
        {
            Debug.Assert(!string.IsNullOrEmpty(pathPattern), "pathPattern should not be null or empty.");

            if (!pathPattern.Contains('*') && !pathPattern.Contains('?'))
            {
                // Return the literal path if there are no wildcards.
                return new string[] { pathPattern };
            }
            else
            {
                // We need to split the directory and the file name pattern
                // apart so Directory.GetFiles can search.
                string directory = Path.GetDirectoryName(pathPattern);
                Ensure(
                    !directory.Contains('*') && !directory.Contains('?'),
                    "Path pattern {0} does not support wildcards in the directory name.",
                    pathPattern);
                string pattern = Path.GetFileName(pathPattern);
                return Directory.GetFiles(directory, pattern, SearchOption.TopDirectoryOnly);
            }
        }

        /// <summary>
        /// Merge the modules into a library.
        /// </summary>
        /// <param name="definitionPath">Path to the definition.</param>
        private static void MergeModulesIntoLibrary(string definitionPath)
        {
            Ensure(
                File.Exists(definitionPath),
                "Library definition {0} not found.",
                definitionPath);

            // Parse the library definition
            XElement library = XDocument.Load(definitionPath).Root;
            Ensure(
                library.Name == "Library",
                "Library definition expects Library as the root element, not {0}.",              
                library.Name);

            // We support generating multiple library files from a single
            // library definition via conditional inclusion.  To generate a new
            // file, simply add a {Condition}Path="..." to your library with
            // the name of your condition add a Condition="{Condition}"
            // attribute to any additional modules you want included in that
            // generated library.  Note that we only support adding modules
            // because our scenarios here are very limited (specifically we
            // want to add a single module to either expose the require method
            // so VS can pick it up correctly for intellisense or to expose
            // all of the internal modules so they can be accessed by unit
            // testS...  and we want to be able to do this without maintaining
            // three copies of the *.library file that all need to be updated
            // when a new module is added).
            // 
            // We find all the conditions by iterating the attributes and 
            // getting everything that ends in Path before trimming off the
            // suffix.  Note that it will leave the empty string as a condition
            // which is used to generate the default library.
            IEnumerable<string> conditions = 
                library
                .Attributes()
                .Select(a => a.Name.ToString())
                .Where(n => n.EndsWith("Path", StringComparison.Ordinal))
                .Select(n => n.Substring(0, n.Length - 4));
            Ensure(conditions.Count() > 0, "Library root should specify a Path attribute.");
            foreach (string scriptCondition in conditions)
            {
                StringBuilder script = new StringBuilder();
                List<string> entrypoints = new List<string>();
                List<string> definedModules = new List<string>();
                List<string> definedResources = new List<string>();

                // Generate the library
                WriteHeader(script, definitionPath, GetAttribute(library, "Copyright"), GetAttribute(library, "FileVersion"));
                
                foreach (XElement child in library.Elements("Resource"))
                {
                    string resourceCondition = GetAttribute(child, "Condition");
                    if (ScriptConditionSatisifesModuleCondition(scriptCondition, resourceCondition))
                    {
                        WriteResource(script, child, definedResources);
                    }
                }

                foreach (XElement child in library.Elements("Module"))
                {
                    string moduleCondition = GetAttribute(child, "Condition");
                    if (ScriptConditionSatisifesModuleCondition(scriptCondition, moduleCondition))
                    {
                        WriteModule(script, child, definedModules, entrypoints);
                    }
                }

                WriteFooter(script, entrypoints);

                // Save the generated library code
                string outputPath = Path.Combine(
                    Path.GetDirectoryName(definitionPath),
                    GetAttribute(library, scriptCondition + "Path"));
                File.WriteAllText(outputPath, script.ToString(), Encoding.UTF8);
            }
        }

        /// <summary>
        /// Determines whether the module should be merged into the specified script, by checking
        /// the script name against the module condition string.
        /// For example, the script Gamma matches the condition Alpha|Beta|Gamma, but Delta does not.
        /// </summary>
        /// <param name="scriptCondition">The script being built</param>
        /// <param name="moduleCondition">The condition attribute from the module being considered</param>
        /// <returns>True if the module should be included</returns>
        private static bool ScriptConditionSatisifesModuleCondition(string scriptCondition, string moduleCondition)
        {
            // Only include this module if it either has no Condition, or its Condition matches the current
            // conditional version of the library we're generating.
            if (moduleCondition == null)
            {
                return true;
            }

            string[] matchingScripts = moduleCondition.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
            return matchingScripts.Contains(scriptCondition, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Write the library definition header including the module cache and
        /// implementation of require.
        /// </summary>
        /// <param name="script">The library script being written.</param>
        /// <param name="libraryDefinitionPath">
        /// Path to the definition of the library which is used to provide a
        /// helpful comment about regeneration.
        /// </param>
        /// <param name="copyright">
        /// Optional copyright header to include at the top of the library.
        /// </param>
        private static void WriteHeader(StringBuilder script, string libraryDefinitionPath, string copyright, string fileVersion)
        {
            Debug.Assert(script != null, "script should not be null.");
            Debug.Assert(!string.IsNullOrEmpty(libraryDefinitionPath), "libraryDefinitionPath should not be null or empty");
            Debug.Assert(File.Exists(libraryDefinitionPath), "libraryDefinitionPath should point to an existing library definition.");

            // Include a copyright header
            // The leading "!" character tells minifiers that this is an "important" comment that should not be removed
            if (!string.IsNullOrEmpty(copyright))
            {
                script.AppendLine("// ----------------------------------------------------------------------------");
                script.AppendLine("//! " + copyright);
                script.AppendLine("// ----------------------------------------------------------------------------");
                script.AppendLine();
                script.AppendLine();
            }
            
            // Include a reminder not to edit the generated library file
            script.AppendLine("// WARNING: This code was generated by a tool from the library definition");
            script.AppendLine("//     " + libraryDefinitionPath);
            script.AppendLine("// Do not modify this file directly.  Any changes made to this file will be");
            script.AppendLine("// lost the next time it is regenerated.");
            script.AppendLine();
            script.AppendLine();

            // Take in the global context which will allow some flexibility in
            // sharing modules between different systems (like Windows 8 and
            // node.js) so people don't resort to tacking things onto the
            // window object.
            script.AppendLine("(function (global) {");

            // The module cache starts by mapping top level module names to
            // functions (taking exports as their only parameter), evaluates
            // the functions once they are require-d, and then caches the
            // exports in the mapping (so each module is only "evaluated" one
            // time).
            // 
            // Note that $__modules__ will appear as another free variable in
            // the module implementations, but this is an implementation
            // detail that shouldn't be relied on (except of course for testing
            // purposes as it allows us to simulate InternalsVisibleTo by
            // exposing all of the registered modules and their content).
            script.AppendLine("    /// <field name=\"$__modules__\">");
            script.AppendLine("    /// Map module names to either their cached exports or a function which");
            script.AppendLine("    /// will define the module's exports when invoked.");
            script.AppendLine("    /// </field>");
            script.AppendLine("    var $__modules__ = { };");
            script.AppendLine("    var $__fileVersion__ = \"" + fileVersion + "\";");
            script.AppendLine("    ");
            // Implement require in the top level scope so it will appear as a
            // free variable in all of the module definitions.
            script.AppendLine("    function require(name) {");
            script.AppendLine("        /// <summary>");
            script.AppendLine("        /// Require a module's exports.");
            script.AppendLine("        /// </summary>");
            script.AppendLine("        /// <param name=\"name\" type=\"String\">");
            script.AppendLine("        /// The name of the module.  Note that we don't support full CommonJS");
            script.AppendLine("        /// Module specification names here - we only allow the name of the");
            script.AppendLine("        /// module's file without any extension.");
            script.AppendLine("        /// </param>");
            script.AppendLine("        /// <returns type=\"Object\">");
            script.AppendLine("        /// The exports provided by the module.");
            script.AppendLine("        /// </returns>");
            script.AppendLine();
            
            // Trim "./" prefix from module names (which enables incorporating
            // npm modules that don't dig very deeply in the hierarchy).
            script.AppendLine("        if (name && name.length > 2 && name[0] == '.' && name[1] == '/') {");
            script.AppendLine("            name = name.slice(2);");
            script.AppendLine("        }");
            script.AppendLine();
            
            script.AppendLine("        var existing = $__modules__[name];");

            // If a module has not yet been required, it will be a function
            // that when evaluated will populate the module's exports.
            script.AppendLine("        if (typeof existing == 'function') {");
            
            // We need to setup the exported values object in the module cache
            // before we call the function and start defining the exports.
            // This makes us compliant with the cirucular definition behavior
            // described in the CommonJS Module Spec.  Imagine we have two
            // modules that look like:
            //     a.js:
            //         exports.foo = function() { /* ... */ };
            //         var baz = require('b').baz;
            //         exports.bar = function() { /* use baz in here ... */ };
            //     b.js
            //         var foo = require('a').foo;
            //         exports.baz = function() { /* use foo in here ... */ };
            // When b.js requires a.js, we want to have everything in a.js that
            // is exported before the require that lead to the circular
            // dependency available to b.js.
            script.AppendLine("            var exports = { };");
            script.AppendLine("            $__modules__[name] = exports;");
            script.AppendLine("            existing(exports);");
            script.AppendLine("            return exports;");

            // Otherwise just return the cached exports or throw an exception
            // if a non-existant module was required.
            script.AppendLine("        } else if (typeof existing == 'object') {");
            script.AppendLine("            return existing;");
            script.AppendLine("        } else {");
            script.AppendLine("            throw 'Unknown module ' + name;");
            script.AppendLine("        }");
            script.AppendLine("    }");
            script.AppendLine();
        }

        /// <summary>
        /// Add the module definition to the library.
        /// </summary>
        /// <param name="script">The library script being written.</param>
        /// <param name="module">Module XML element.</param>
        /// <param name="definedModules">
        /// List of defined modules used to ensure no duplication.
        /// </param>
        /// <param name="entrypoints">
        /// Collection of modules that should be automatically required as
        /// entry points for the merged library script.
        /// </param>
        private static void WriteModule(StringBuilder script, XElement module, List<string> definedModules, List<string> entrypoints)
        {
            Debug.Assert(script != null, "script should not be null.");
            Debug.Assert(module != null, "module should not be null.");
            Debug.Assert(entrypoints != null, "entrypoints should not be null.");

            Ensure(
                module.Name == "Module",
                "Library definition expects Module child elements, not {0}.",
                module.Name);
            string path = GetAttribute(module, "Path");
            Ensure(
                !string.IsNullOrEmpty(path),
                "Module element should specify a Path attribute.");

            // Expand any wildcards in the path name (which lets us include all
            // the *.js modules in a given directory, etc., which makes it easy
            // to do things like pull all the unit tests into a module).
            foreach (string modulePath in ExpandPath(path))
            {
                Ensure(
                    File.Exists(modulePath),
                    "Module Path {0} does not exist.",
                    modulePath);

                // We only support a so called "top level" name which is just
                // the extension-less file name.  Full CommonJS module support
                // would require us working with relative paths (which wouldn't
                // be too hard - we'd just need to either store or precompute
                // the relative path to this particular module so we could
                // munge that with any relative paths provided to require...
                // i.e., mimic a small part of the file system in our
                // $__modules__ cache).  Given that we're only dealing with a
                // static set of modules known at compilation time, the benefit
                // that relative paths add outweighs the complexity they would
                // introduce.
                string topLevelModuleName = string.IsNullOrEmpty(GetAttribute(module, "Name"))
                    ? Path.GetFileNameWithoutExtension(modulePath)
                    : GetAttribute(module, "Name");

                // Ensure there are no duplicate module names.  This would most
                // likely happen if the library included wildcard searches that
                // created a conflict by including two files with the same name
                // but at different parts of the file system.  This is a
                // limitation of not encoding file system details in our module
                // names.
                Ensure(
                    !definedModules.Contains(topLevelModuleName),
                    "Cannot have multiple modules named '{0}'.",
                    topLevelModuleName);
                definedModules.Add(topLevelModuleName);

                // Determine whether this module is an entry point.
                // 
                // An entry point corresponds conceptually to the script you
                // would pass when starting the node.js process.  Without entry
                // points, the modules wouldn't actually do anything since no
                // one would be able to require them (as require's hidden in a
                // new scope context so we don't leak details of the module
                // system to regular code).
                // 
                // Entry points are implemented by just adding an explicit
                // require after all the modules have been defined.  We allow
                // multiple entry points (processed in the order listed in the
                // library definition file) because it's technically simple and
                // makes our approach to exposing internal details for testing
                // a little easier.
                bool isEntrypoint = false;
                if (bool.TryParse(GetAttribute(module, "EntryPoint"), out isEntrypoint) &&
                    isEntrypoint)
                {
                    entrypoints.Add(topLevelModuleName);
                }

                // Provide the module implementation by wrapping it in a
                // function which will provide it the exports object to
                // populate.  The function will only be evaluate when the
                // module is required.
                script.AppendFormat("    $__modules__{0} = function (exports) {{",
                    // We prefer to use "dot notation" where possible, but will
                    // default to using the object as a dictionary if the name
                    // contains other invalid identifier characters.  (This is
                    // specifically to make sure JsHint doesn't complain at the
                    // generated library.)
                    topLevelModuleName.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '$') ?
                        "." + topLevelModuleName :
                        "['" + topLevelModuleName + "']");
                script.AppendLine();

                // Filter out any Visual Studio intellisense references at the
                // top of the file as these are 1) only helpful for development
                // (given the way we hack around the lack of intellisense for
                // modules) and 2) unused since they're not at the very top of
                // the library file.
                IEnumerable<string> lines =
                    File.ReadLines(modulePath)
                    .SkipWhile(line =>
                        Regex.IsMatch(line, @"^\/\/\/\s*\<reference") ||
                        string.IsNullOrEmpty(line.Trim()));
                foreach (string line in lines)
                {
                    script.AppendLine("        " + line);
                }
                script.AppendLine("    };");
                script.AppendLine();
            }
        }

        /// <summary>
        /// Add the resources for the language/region to the library.
        /// </summary>
        /// <param name="script">The library script being written.</param>
        /// <param name="module">Resource XML element.</param>
        /// <param name="definedModules">
        /// List of defined resources used to ensure no duplication.
        /// </param>
        private static void WriteResource(StringBuilder script, XElement resource, List<string> definedResources)
        {
            Debug.Assert(script != null, "script should not be null.");
            Debug.Assert(resource != null, "resource should not be null.");

            Ensure(
                resource.Name == "Resource",
                "Library definition expects Resource child elements, not {0}.",
                resource.Name);

            string path = GetAttribute(resource, "Path");
            Ensure(
                !string.IsNullOrEmpty(path),
                "Resource element should specify a Path attribute.");
            Ensure(
                File.Exists(path),
                "Resource Path {0} does not exist.",
                path);

            string languageTag = GetAttribute(resource, "LanguageTag");
            Ensure(
                !string.IsNullOrWhiteSpace(languageTag),
                "Resource element must specify a LanguageTag attribute");
            Ensure(
                !definedResources.Contains(languageTag),
                "Cannot have multiple resources with the LangugageTag '{0}'.",
                languageTag);
            definedResources.Add(languageTag);

            // We have to make sure a Resources object is defined before
            // we can add members to it.
            if (definedResources.Count == 1)
            {
                script.AppendLine("    $__modules__.Resources = { };");
                script.AppendLine();
            }

            // Add the language resjson to the Resources object as a Javascript object
            script.AppendFormat("    $__modules__.Resources['{0}'] =", languageTag);

            IEnumerable<string> lines = File.ReadLines(path)
                                            .SkipWhile(line => string.IsNullOrEmpty(line.Trim()));
            foreach (string line in lines)
            {
                script.AppendLine();
                script.Append("        " + line);
            }

            script.AppendLine(";");
            script.AppendLine();
        }

        /// <summary>
        /// Finish the library definition by require-ing all of the entry point
        /// modules and closing the global scope.
        /// </summary>
        /// <param name="script">The library script being written.</param>
        /// <param name="entrypoints">
        /// Collection of modules that should be automatically required as
        /// entry points for the merged library script.
        /// </param>
        private static void WriteFooter(StringBuilder script, List<string> entrypoints)
        {
            Debug.Assert(script != null, "script should not be null.");
            Debug.Assert(entrypoints != null, "entrypoints should not be null.");

            // Require each of the entry point modules in the order provided by
            // the library definition.  Forcing require-s of these modules
            // allows the otherwise static definitions to set things up, define
            // globally accessible values, cause side effects, etc.
            foreach (string entrypoint in entrypoints)
            {
                script.AppendLine("    require('" + entrypoint + "');");
            }

            // Pass in the current object as the 'global' free variable and
            // In Windows 8 and browser contexts, 'this' will refer to the
            // window object. In Node.js, exports should be used to provide
            // public APIs (and 'this' will be undefined).
            script.AppendLine("})(this || exports);");
        }
    }
}
