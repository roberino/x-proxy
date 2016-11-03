# Proxy analysis user interface - based on angular-seed — the seed for AngularJS apps

### Install Dependencies

We have two kinds of dependencies in this project: tools and angular framework code.  The tools help
us manage and test the application.

* We get the tools we depend upon via `npm`, the [node package manager][npm].
* We get the angular code via `bower`, a [client-side code package manager][bower].

We have preconfigured `npm` to automatically run `bower` so we can simply do:

```
npm install
```

Behind the scenes this will also call `bower install`.  You should find that you have two new
folders in your project.

* `node_modules` - contains the npm packages for the tools we need
* `app/bower_components` - contains the angular framework files

*Note that the `bower_components` folder would normally be installed in the root folder but
angular-seed changes this location through the `.bowerrc` file.  Putting it in the app folder makes
it easier to serve the files by a webserver.*

## Directory Layout

```
app/                    --> all of the source files for the application
  app.css               --> default stylesheet
  components/           --> all app specific modules
  *-view/               --> view code and template
  app.js                --> main application module
  index.html            --> app layout file (the main html template file of the app)
```

## Contact

For more information on AngularJS please check out http://angularjs.org/

[bower]: http://bower.io
[git]: http://git-scm.com/
[node]: https://nodejs.org
[npm]: https://www.npmjs.org/