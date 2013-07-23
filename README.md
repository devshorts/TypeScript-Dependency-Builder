TypeScript .d.ts and html javascript scripts dependency builder
===================================

This project does two things

1. Populates an html page with javascript script blocks based on a configured list of dependencies that contain the js source files that should be put into the html page.
2. Builds out an aggregate .d.ts file for a group of folders so you can use a single typescript reference path instead of having to manually manage your reference paths

Lets look at each thing individually.  

Usage
---

```
<Usage>: TypescriptBuilders.exe <path to depends.json>
```

Configuration
----
First, the application has a dependency configuration.  Here is a sample one (that I've been using it for).  

```
{	
	"Definitions":[
		{
			"RootFolder": "../../../../../../src/html/main/js",
			"SubFolders":[  
							"def",
                            "models",
                            "common",
                            "controllers",
                            "directives",
                            "services",
                            "filters",
                            "data"
						],
			"NameOfDefFile":"_all.d.ts"
		},
		{
			"RootFolder": "../../../../../../src/html/shared/js",
			"SubFolders":[  							
                            "common",
                            "data",
                            "interfaces",
                            "services"
						],
			"NameOfDefFile":"_all.d.ts"
		}
	],
    "Dependencies":[
      {
        "Name":"App",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\main",
		"IndexPage":"Default.aspx",
		"DependsOnNames":["Shared"],
		"ExcludeFolders":["tests", "locale"],
		"ExcludeNames":["app.compiled.min.js", "app.compiled.js"]
      },
      {
        "Name":"SharedCommon",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\common",	
		"DependsOnNames":[]
      },
	  {
        "Name":"SharedData",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\data",	
		"DependsOnNames":[]
      },
	  {
        "Name":"SharedServices",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\services",	
		"DependsOnNames":[]
      },	  
	  {
        "Name":"Shared",		
		"DependsOnNames":["SharedCommon", "SharedData", "SharedServices"]
      },
	  {
        "Name":"E2E Mock",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\main\tests",
		"IndexPage":"Default.aspx",
		"DependsOnNames":["App", "Shared" ],
		"ExcludeFolders":["e2e", "unitTests"]
      }	   
    ]
}
```

Build html page javascript dependencies
---

Looking at the second block:

```
{	
    "Dependencies":[
      {
        "Name":"App",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\main",
		"IndexPage":"Default.aspx",
		"DependsOnNames":["Shared"],
		"ExcludeFolders":["tests", "locale"],
		"ExcludeNames":["app.compiled.min.js", "app.compiled.js"]
      },
      {
        "Name":"SharedCommon",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\common",	
		"DependsOnNames":[]
      },
	  {
        "Name":"SharedData",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\data",	
		"DependsOnNames":[]
      },
	  {
        "Name":"SharedServices",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\shared\js\services",	
		"DependsOnNames":[]
      },	  
	  {
        "Name":"Shared",		
		"DependsOnNames":["SharedCommon", "SharedData", "SharedServices"]
      },
	  {
        "Name":"E2E Mock",
		"RelativePath":" ..\..\..\..\..\..\..\..\src\html\main\tests",
		"IndexPage":"Default.aspx",
		"DependsOnNames":["App", "Shared" ],
		"ExcludeFolders":["e2e", "unitTests"]
      }	   
    ]
}
```

This section lets you define all the related javascript files that are under the relative path of an application as well as any separate dependencies.  Anything that has an index page defined will get their index page updated.  The idea here is that when you are working on an application (lets say using angularjs) and you are adding lots of JS files that need to be frequently added to the index page for loading, to automate that task for you.  

The index page needs to have a comment block that looks like this:

```
<!-- GENERATED LOCAL DATA OUT START -->

<!-- GENERATED LOCAL DATA OUT END -->
```

And all the javascript script blocks (properly formatted to be relative to the index page path) will be inserted there.  It doesn't matter if the comments are in the header, footer, middle of the page, whatever.  

In the example above I was working on an angularJS application that had shared libraries, shared services, shared common files, a main app, and an end to end angular unit test page.  The main aspx page and the e2e page needed to have all the javascript files loaded in with proper relative paths.  This was a pain to do by hand, hence why I wrote this tool. 

You are also able to set up excludes by folder and by specific file names.  Right now the excludes don't handle regex, but thats easy to update.

Also, the ordering of the script files in the page blocks uses a custom sort, which you can change if you want. The order is defined by file names that contain these strings in this order:

```
"base", 
"data", 
"model", 
"services", 
"interceptor", 
"controllers", 
"directives", 
"filters", 
"app", 
"tests", 
"unittests"
```

But it'd be easy to pass in that later.

Building Typescript Definition Files
---

The first part of the configuration looks like this:

```
"Definitions":[
		{
			"RootFolder": "../../../../../../src/html/main/js",
			"SubFolders":[  
							"def",
                            "models",
                            "common",
                            "controllers",
                            "directives",
                            "services",
                            "filters",
                            "data"
						],
			"NameOfDefFile":"_all.d.ts"
		},
		{
			"RootFolder": "../../../../../../src/html/shared/js",
			"SubFolders":[  							
                            "common",
                            "data",
                            "interfaces",
                            "services"
						],
			"NameOfDefFile":"_all.d.ts"
		}
]
```

This configures the definitions builder on how to build out a definitions file that would look like this, in a file called `_all.d.ts`:

```
/// <reference path="../def/definitions.d.ts" />

// --------COMMON--------
/// <reference path="./common/model/AngularGlobal.ts"/>
/// <reference path="./common/model/ControllerBase.ts"/>
/// <reference path="./common/model/DirectiveBase.ts"/>
/// <reference path="./common/model/FilterBase.ts"/>
/// <reference path="./common/model/ServiceBase.ts"/>
/// <reference path="./common/utils/MiscUtil.ts"/>
/// <reference path="./common/utils/StringUtil.ts"/>
/// <reference path="./common/utils/VideoUtil.ts"/>
// --------END COMMON--------

// --------DATA--------
/// <reference path="./data/AuthenticationChallengeData.ts"/>
/// <reference path="./data/LoginAuthenticationResponseData.ts"/>
/// <reference path="./data/PublicKeyData.ts"/>
// --------END DATA--------

// --------INTERFACES--------
/// <reference path="./interfaces/IResponseInfo.ts"/>
// --------END INTERFACES--------

// --------SERVICES--------
/// <reference path="./services/LogWebService.ts"/>
/// <reference path="./services/UnsafeWebService.ts"/>
// --------END SERVICES--------
```

Now in your typescript files you just need to reference 

```
/// <reference path="../_all.d.ts" />
```

Without having to worry about updating it with all your files. You can create multiple definition aggregates.  Just like in the index page builder, the builder looks for 

```
// --------IDENTIFIER--------
	... stuff ...
// --------END IDENTIFIER--------
```

To build into. The identifier is the name of the folder that it found those `.ts` files in.  If  the `.d.ts` aggregate doesn't exist, the tool will create one for you.

Also, you can add whatever you want outside of those blocks and they won't be affected. So you can mix generated and manual definitions.