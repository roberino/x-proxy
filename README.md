[![Build Status](https://travis-ci.org/roberino/x-proxy.svg?branch=master)](https://travis-ci.org/roberino/x-proxy)


# eXProxy

HTTP proxy for testing and API analysis.

# Example usage

```cs

var baseDir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "data"));

using (var startup = new Startup(baseDir, "http://localhost:8001/", new [] { "http://my.site.url/" }, "http://localhost:8081/", "http://localhost:8080/"))
{
    startup.Start();

    Console.ReadKey();

    startup.Stop();
}

// Traffic to http://localhost:8001/ will be proxied to http://my.site.url/
// and viewed via http://localhost:8080/
			
``` 

# Acknowledgements...

...to these awesome projects:

* https://github.com/n3-charts/line-chart
* http://getbootstrap.com/
* https://github.com/angular/angular-seed
* https://github.com/nickperkinslondon/angular-bootstrap-nav-tree
* https://code.google.com/p/google-diff-match-patch/
