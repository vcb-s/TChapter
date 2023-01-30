TChapter
==========

A .NET library for parsing chapter file.

Usage
-----

### Supported file type

- OGM(`.txt`)
- XML(`.xml`)
- MPLS from BluRay(`.mpls`)
- IFO from DVD(`.ifo`)
- XPL from HDDVD(`.xpl`)
- CUE plain text or embedded(`.cue`, `.flac`, `.tak`)
- Matroska file(`.mkv`, `.mka`)
- Mp4 file(`.mp4`, `.m4a`, `.m4v`)
- WebVTT(`.vtt`)

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
If some chapter types always contain only one chapter, it will return a more specific type `SingleChapterData`.
```C#
foreach (var chapter in (data as SingleChapterData).Chapters)
{
    Console.WriteLine(chapter);
}
```

The chapter type like MPLS may contain separate chapters, you can combine these chapters by calling `chapters.CombineChapter()`.

-----
### Saving

You should call the save method as below, see the code document for more information.

```C#
data.Save(ChapterTypeEnum.XML, "path/to/save/chapter.xml");
```


## License

Distributed under the GPLv3+ License. See LICENSE for more information.
