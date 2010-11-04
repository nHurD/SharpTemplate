# Sharp Template
 A text templating system based on the old PTemplate from the PHP world.

## A Brief Example

### Template Format

 Content.tmpl:
>	<html>
>		<head>
>			<title>{pageTitle}</title>
>		</head>
>		<body>
>			<h1>This is a Test!</h1>
>			<p>{body}</p>
>		</body>
>	</html>


### C# Code Listing
* Start out by creating a new instance of Template:
	Template t = new Template();

* Alternatively, you can specify a template directory: 
	Template t = new Template("templates");

* Once the class is initialized, make sure the top most tag `OUTPUT` is assigned or parsed:
	t.parseTag("OUTPUT","Content.tmpl");	

* Assign some values:
	t.assignTag("pageTitle", "Test Parse");
	t.assignTag("body", "Foo bar");

* Process:
	string result = t.printTemplate();

