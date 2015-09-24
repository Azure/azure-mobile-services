var remapify = require('remapify');

function definePlatformMappings(mappings) {
    return function(b) {
        b.plugin(remapify, mappings);
    };
}

var sdkExports = {
    web: [ 
        ['./src/Utilities/Extensions.js', {expose: 'Extensions'}],
        ['./src/Utilities/Validate.js', {expose: 'Validate'}],
        // Expose Platform.js as Platforms/Platform to be consistent with how it is referenced within the SDK bundle
        ['./src/Platforms/web/Platform.js', {expose: 'Platforms/Platform'}] 
    ],
    winjs: [ 
        ['./src/Utilities/Extensions.js', {expose: 'Extensions'}],
        ['./src/Utilities/Validate.js', {expose: 'Validate'}],
        // Expose Platform.js as Platforms/Platform to be consistent with how it is referenced within the SDK bundle
        ['./src/Platforms/winjs/Platform.js', {expose: 'Platforms/Platform'}] 
    ]
};

var sdkImports = [
    'Extensions',
    'Validate',
    'Platforms/Platform'
];

var sdkBrowserifyOptions = {
    web: {
        preBundleCB: definePlatformMappings( [ { src: '**/*.js', cwd: __dirname + '/src/Platforms/web', expose: 'Platforms' } ] )
    },
    cordova: {
        preBundleCB: definePlatformMappings( [ { src: '**/*.js', cwd: __dirname + '/src/Platforms/web', expose: 'Platforms' } ] )
    },
    winjs: {
        preBundleCB: definePlatformMappings( [ { src: '**/*.js', cwd: __dirname + '/src/Platforms/winjs', expose: 'Platforms' } ] )
    }
};

/// <vs BeforeBuild='default' />
module.exports = function(grunt) {
  // Project configuration.
  grunt.initConfig({
    pkg: grunt.file.readJSON('package.json'),
    files: {
      core: [
        'src/MobileServiceClient.js',
      ],
      web: [
        '<%= files.core %>',
      ],
      cordova: [
        'src/Platforms/cordova/MobileServiceSQLiteStore.js',
        '<%= files.core %>',
      ],
      winjs: [
        '<%= files.core %>',
      ],
      intellisense: [
        'src/Internals/DevIntellisense.js',
      ],
      testcore: [
          'test/winJS/tests/utilities/*.js',
          'test/winJS/tests/unit/*.js',
          'test/winJS/tests/functional/*.js',
      ],
      all: [
        'Gruntfile.js',
        'src/**/*.js',
        'test/**/*.js',
        '!**/[gG]enerated/*.js',
        '!test/cordova/platforms/**',
        '!test/**/bin/**',
        '!test/**/plugins/**',
        '!**/node_modules/**',
        '!**/MobileServices.*.js',
        '!**/External/**'
      ]
    },    
    jshint: {
        all: '<%= files.all %>'
    },    
    concat: {
      constants: {
        options: {
          banner: header + 
                  '\nexports.FileVersion = \'<%= pkg.version %>\';\n' +
                  '\nexports.Resources = {};\n',
          process: wrapResourceFile,
        },
        src: ['src/Strings/**/Resources.resjson'],
        dest: 'src/Generated/Constants.js'
      },
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
    },
    copy: {
      cordovaTest: {
        files: [
          {src: ['src/Generated/MobileServices.Web.Internals.js'], dest: 'test/cordova/www/js/Generated/MobileServices.Web.Internals.js'},
          {src: ['test/web/css/styles.css'], dest: 'test/cordova/www/css/Generated/styles.css'},
          {src: ['**'], dest: 'test/cordova/www/js/External/qunit/', cwd: 'node_modules/qunitjs/qunit', expand: true}
        ]
      }
    },
    browserify: {
        options: {
            banner: header
        },
        web: {
            src: '<%= files.web %>',
            dest: './src/Generated/MobileServices.Web.js',
            options: sdkBrowserifyOptions.web
        },
        webInternals: {
            src: '<%= files.web %>',
            dest: './src/Generated/MobileServices.Web.Internals.js',
            options: {
                preBundleCB: sdkBrowserifyOptions.web.preBundleCB,
                require: sdkExports.web
            }
        },
        cordova: {
            src: '<%= files.cordova %>',
            dest: './src/Generated/MobileServices.Cordova.js',
            options: sdkBrowserifyOptions.cordova
        },
        cordovaInternals: {
            src: '<%= files.cordova %>',
            dest: './src/Generated/MobileServices.Cordova.Internals.js',
            options: {
                preBundleCB: sdkBrowserifyOptions.cordova.preBundleCB,
                require: sdkExports.web
            }
        },
        winjs: {
            src: '<%= files.winjs %>',
            dest: './src/Generated/MobileServices.js',
            options: sdkBrowserifyOptions.winjs
        },
        winjsInternals: {
            src: '<%= files.winjs %>',
            dest: './src/Generated/MobileServices.Internals.js',
            options: {
                preBundleCB: sdkBrowserifyOptions.winjs.preBundleCB,
                require: sdkExports.winjs,
            }
        },
        intellisense: {
            src: [
                '<%= files.winjs %>',
                '<%= files.intellisense %>'
            ],
            dest: './src/Generated/MobileServices.DevIntellisense.js',
            options: sdkBrowserifyOptions.winjs
        },
        webTest: {
            src: [
                './test/web/js/TestFrameworkAdapter.js',
                './test/web/js/TestClientHelper.js',
                '<%= files.testcore %>'
            ],
            dest: './test/web/Generated/Tests.js',
            options: {
                external: sdkImports,
            }
        },
        cordovaTest: {
            src: [
                './test/web/js/TestFrameworkAdapter.js',
                './test/web/js/TestClientHelper.js',
                '<%= files.testcore %>'
            ],
            dest: './test/cordova/www/js/Generated/Tests.js',
            options: {
                external: sdkImports,
            }
        },
        winjsTest: {
            src: [
                'test/winJS/tests/TestFramework.js',
                'test/winJS/tests/TestInterface.js',
                '<%= files.testcore %>'
            ],
            dest: './test/winJS/Generated/Tests.js',
            options: {
                external: sdkImports,
            }
        }
    },
    watch: {
        files: '<%= files.all %>',
        tasks: ['concat', 'uglify', 'copy', 'browserify', 'jshint']
    }
  });

  // Load the plugin that provides the "uglify" task.
  grunt.loadNpmTasks('grunt-contrib-uglify');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.loadNpmTasks('grunt-contrib-concat');
  grunt.loadNpmTasks('grunt-contrib-copy');
  grunt.loadNpmTasks('grunt-browserify');
  grunt.loadNpmTasks('grunt-contrib-watch');
    
  // Default task(s).
  grunt.registerTask('default', ['concat', 'browserify', 'uglify', 'copy', 'jshint']);
};

var header = '// ----------------------------------------------------------------------------\n' +
             '// Copyright (c) Microsoft Corporation. All rights reserved\n' +
             '// <%= pkg.name %> - v<%= pkg.version %>\n' +
             '// ----------------------------------------------------------------------------\n';

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

  var language = filepath.replace('src/Strings/', '').replace('/Resources.resjson', '');

  return '\nexports.Resources[\'' + language + '\'] = ' +
         src + ';';
}

