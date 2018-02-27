TChapter
==========

A .NET library for parsing chapter file.

Overview
--------

- [Usage](#usage)
  - [Parsing](#parsing)
  - [Saving](#saving)
  - [Logging](#logging)

Usage
-----
### Parsing

```C#
// Initialize the parser with specific parser
var parser = new OGMParser();
var data = parser.Parse(@"path/to/chapter.txt");

// the data can always be accessed as follow
foreach (var chapter in data)
{
    foreach (var item in chapter.Chapters)
    {
        Console.WriteLine(item);
    }
    Console.WriteLine();
}
```

If some types of chapter which will always include only one chapter, it will return a more specific type `SingleChapterData`.
```C#
foreach (var chapter in (data as SingleChapterData).Chapters)
{
    Console.WriteLine(chapter);
}
```

The chapter type such as MPLS may include separated chapters, you can combine these chapters by `chapters.CombineChapter()`

-----
### Saving

You should call the saving method like below, refer to code document for more infomation.

```C#
data.Save(ChapterTypeEnum.XML, "path/to/save/chapter.xml");
```

-----
### Logging

You can access the [SimpleLogger](https://github.com/jirkapenzes/SimpleLogger) for logging usage.
