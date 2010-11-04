SharpTemplate.dll: iTemplate.cs TagInfo.cs Template.cs
	gmcs -target:library -out:SharpTemplate.dll -r:System.Data -r:System iTemplate.cs TagInfo.cs Template.cs

clean:
	rm SharpTemplate.dll
