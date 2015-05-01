    /// <field name="$__modules__">
    /// Map module names to either their cached exports or a function which
    /// will define the module's exports when invoked.
    /// </field>
    var $__modules__ = { };
    
    function require(name) {
        /// <summary>
        /// Require a module's exports.
        /// </summary>
        /// <param name="name" type="String">
        /// The name of the module.  Note that we don't support full CommonJS
        /// Module specification names here - we only allow the name of the
        /// module's file without any extension.
        /// </param>
        /// <returns type="Object">
        /// The exports provided by the module.
        /// </returns>

        if (name && name.length > 2 && name[0] == '.' && name[1] == '/') {
            name = name.slice(2);
        }

        var existing = $__modules__[name];
        if (typeof existing == 'function') {
            var exports = { };
            $__modules__[name] = exports;
            existing(exports);
            return exports;
        } else if (typeof existing == 'object') {
            return existing;
        } else {
            throw 'Unknown module ' + name;
        }
    }