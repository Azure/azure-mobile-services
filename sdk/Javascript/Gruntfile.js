/// <vs BeforeBuild='default' />
module.exports = function(grunt) {
  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    files: {
      resources: [
        'src/Strings/**/Resources.resjson'
      ],
      core: [
        'src/Utilities/Extensions.js',
        'src/MobileServiceClient.js',
        'src/MobileServiceTable.js',
        'src/MobileServiceLogin.js',
        'src/Push/Push.js',
        'src/Utilities/Validate.js',
        'src/External/queryjs/lib/*.js',
        'src/External/esprima/esprima.js'
      ],
      web: [
        'src/Platforms/Platform.Web.js',
        'src/Generated/MobileServices.Core.js',
        'src/Transports/*.js',
        'src/LoginUis/*.js',
        'src/Utilities/PostMessageExchange.js',
        'src/Utilities/Promises.js'
      ],
      winjs: [
        'src/LoginUis/WebAuthBroker.js',
        'src/Platforms/Platform.WinJS.js',
      ],
      node: [
        'src/Internals/NodeExports.js',
      ],
      Internals: [
        'src/Internals/InternalsVisible.js',
      ],
      Intellisense: [
        'src/Internals/DevIntellisense.js',
      ],
    },    
    jshint: {
        all: ['Gruntfile.js', 'src/**/*.js', '!src/External/**/*.js', '!src/Generated/*.js', 'test/**/*.js', '!test/**/bin/**', '!**/MobileServices.*.js']
    },    
    concat: {
      options: {
        stripBanners: true,
        banner: header,
        process: wrapModule,
        footer: footer
      },
      resources: {
        options: {
          banner: '\n\t$__modules__.Resources = { };\n\n',
          process: wrapResourceFile,
          footer: ''
        },
        src: ['<%= files.resources %>'],
        dest: 'src/Generated/Resources.js'
      },
      web: {
        src: ['src/Require.js', 'src/Generated/Resources.js', '<%= files.core %>', '<%= files.web %>'],
        dest: 'src/Generated/MobileServices.Web.js'
      },
      webinternals: {
        options: {
          footer: '\n\trequire(\'InternalsVisible\');' + footer
        },
        src: ['src/Require.js', 'src/Generated/Resources.js', '<%= files.Internals %>', '<%= files.core %>', '<%= files.web %>'],
        dest: 'src/Generated/MobileServices.Web.Internals.js'
      },
      winjs: {
        src: ['src/Require.js', '<%= files.core %>', '<%= files.winjs %>'],
        dest: 'src/Generated/MobileServices.js'
      },    
      winjsinternals: {
        options: {
          footer: '\n\trequire(\'InternalsVisible\');' + footer
        },
        src: ['src/Require.js', '<%= files.Internals %>', '<%= files.core %>', '<%= files.winjs %>'],
        dest: 'src/Generated/MobileServices.Internals.js'
      },
      Intellisense: {
        options: {
          footer: '\n\trequire(\'DevIntellisense\');' + footer
        },
        src: ['src/Require.js', '<%= files.core %>', '<%= files.winjs %>', '<%= files.Intellisense %>'],
        dest: 'src/Generated/MobileServices.DevIntellisense.js'
      }
    },
    uglify: {
      options: {
          banner: '//! Copyright (c) Microsoft Corporation. All rights reserved. <%= pkg.name %> v<%= pkg.version %>\n',
          mangle: false
      },
      web: {
        src: 'src/Generated/MobileServices.Web.js',
        dest: 'src/Generated/MobileServices.Web.min.js'
      },
      winjs: {
        src: 'src/Generated/MobileServices.js',
        dest: 'src/Generated/MobileServices.min.js'
      }
    }
  });

  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.loadNpmTasks('grunt-contrib-concat');

  // Default task(s).
  grunt.registerTask('default', ['jshint', 'concat', 'uglify']);
};

var header = '// ----------------------------------------------------------------------------\n' +
             '// Copyright (c) Microsoft Corporation. All rights reserved\n' +
             '// <%= pkg.name %> - v<%= pkg.version %>\n' +
             '// ----------------------------------------------------------------------------\n' +
             '\n' +
             '(function (global) {\n' +
             '\tvar $__fileVersion__ = \'<%= pkg.version %>\';\n',
    footer = '\n\trequire(\'MobileServiceClient\');\n' + 
             '})(this || exports);';

function wrapModule(src, filepath) {
  /// <summary>
  /// Takes a file, and if it should be a module, wraps the code in a module block
  /// </summary>
  /// <param name="src">
  /// Source code of a module file
  /// </param>
  /// <param name="filepath">
  /// Sile path of the module (i.e. src/MobileServicesClient.js)
  /// </param>

  var lastSlash = filepath.lastIndexOf('/'),
      name = filepath.substr(lastSlash+1);

  name = name.substring(0, name.indexOf('.'));
  if (name == 'Require' || name == 'Resources') {
    return src;
  }

  var newSrc = src.replace(/\/\/\/\s<[\w\s=":\\().]+\/>\n/g, '');
  newSrc = '\t\t' + newSrc.replace(/\n/g, '\n\t\t');

  return '\n\t$__modules__.' + name + ' = function (exports) {\n' + newSrc + '\n\t};';
}

function wrapResourceFile(src, filepath) {
  /// <summary>
  /// Takes a resjson file and places it into a module level resources array
  /// with the index corresponding to the language identifier in the file path
  /// </summary>
  /// <param name="src">
  /// Source code of a module file
  /// </param>
  /// <param name="filepath">
  /// File path of the resjson (i.e. src/Strings/en-US/Resources.resjson)
  /// The file name must be in format of <directories>/<locale>/Resources.resjson
  /// </param>

  var language = filepath.replace('src/Strings/', '').replace('/Resources.resjson', ''),
      newSrc = src.replace(/\n/g, '\n\t\t');

  return '\t$__modules__.Resources[\'' + language + '\'] = ' + newSrc + ';';
}