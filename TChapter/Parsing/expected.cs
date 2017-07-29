void Main()
{
    var parser = new ChapterParser();
    MultiChapterData chapters = parser.Parse<ChapterType>("path/to/chapter");
    foreach(var chapter in chapters)
    {
        foreach(var item in chapter)
        {
            Console.WriteLine(item);
        }
    }
}
