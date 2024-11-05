using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
var createRspCommand = new Command("create-rsp", "Enter the desired value for each command option.");
var outPutOption = new Option<FileInfo>(new[] { "--output", "-o" } , () => new FileInfo("command.txt"), // קובץ ברירת מחדל
        "Path to the .rsp file to be created.");
createRspCommand.AddOption(outPutOption);
createRspCommand.SetHandler((FileInfo output) =>
{

    try { 
    string rspContent = "";
    Console.WriteLine("Which language(s) do you want? (cs, js, py, or 'all'):");
    string language = Console.ReadLine();
    rspContent += $"--language {language} ";
        Console.WriteLine("Path to the output file (output.txt):");
        string filePath = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Output file path cannot be empty.");
        }
      FileInfo path = new FileInfo(filePath);
    rspContent += $"--output {filePath} ";

 
    Console.WriteLine("Include source file paths as comments? (true/false):");
    string note = Console.ReadLine();
    if (string.IsNullOrEmpty(note)) note = "false";
    rspContent += $"--note {note} ";

    
    Console.WriteLine("Sort files by 'name' or 'type'?");
    string sort = Console.ReadLine();
    if (string.IsNullOrEmpty(sort)) sort = "name";
    rspContent += $"--sort {sort} ";

 
    Console.WriteLine("Remove empty lines? (true/false):");
    string removeEmptyLines = Console.ReadLine();
    if (string.IsNullOrEmpty(removeEmptyLines)) removeEmptyLines = "false";
    rspContent += $"--removeEmptyLines {removeEmptyLines} ";

 
    Console.WriteLine("Author name (optional):");
    string author = Console.ReadLine();
    if (!string.IsNullOrEmpty(author))
    {
        rspContent += $"--author {author} ";
    }

  
    File.WriteAllText(output.FullName, rspContent);
    Console.WriteLine($"Response file created successfully at {output.FullName}");
}
    catch (Exception ex)
    {
    Console.WriteLine($"Error creating response file: {ex.Message}");
    }
},outPutOption);

var bundleCommand = new Command("bundle", "Bundle Handling of packaging code files into one file.");

var languageOption = new Option<string[]>(
    new[] { "--language", "-l" },
    "A list of programming languages to include code files from. Use 'all' to include all code files."
);


var fileOption = new Option<FileInfo>(
    new[] { "--output", "-o" }
            ,
            () => new FileInfo("output.txt"), 
            "File path and name to bundle the files into."
        );

var noteOption = new Option<bool>(
     new[] { "--note", "-n" }
   ,
    "Include the source file's relative path as a comment in the bundle."
);


var sortOption = new Option<string>(
        new[] { "--sort", "-s" }
   ,
    () => "name",
    "Sort order: 'name' (default) or 'type'."
);


var removeEmptyLinesOption = new Option<bool>(
      new[] { "--removeEmptyLines", "-r" }
   ,
    "Remove empty lines from the code before bundling."
);


var authorOption = new Option<string>(
          new[] { "--author", "-a" }
    ,
    "Name of the author to include in the bundle as a comment."
);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(fileOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);



bundleCommand.SetHandler(
    (string[] language, FileInfo output, bool note, string sort, bool removeEmptyLines, string author) =>
    {
        try
        {
           
            var currentDirectory = Directory.GetCurrentDirectory();
            var allFiles = Directory.GetFiles(currentDirectory, "*.*", SearchOption.AllDirectories)
                .Where(file => !file.Contains("bin") && !file.Contains("obj") && !file.Contains("debug"));
            var filesToBundle = language.Length == 1 && language[0].Equals("all", StringComparison.OrdinalIgnoreCase)
                ? allFiles.ToArray()
                : allFiles.Where(file => language.Any(lang => file.EndsWith($".{lang}", StringComparison.OrdinalIgnoreCase))).ToArray();
            filesToBundle = sort.Equals("type", StringComparison.OrdinalIgnoreCase)
                ? filesToBundle.OrderBy(file => Path.GetExtension(file)).ToArray()
                : filesToBundle.OrderBy(file => Path.GetFileName(file)).ToArray();
            using (var writer = new StreamWriter(output.FullName))
            {
                if (!string.IsNullOrEmpty(author))
                {
                    writer.WriteLine($"// Author: {author}");
                }
                foreach (var file in filesToBundle)
                {
                    if (note)
                    {
                        var relativePath = Path.GetRelativePath(currentDirectory, file);
                        writer.WriteLine($"// Source: {relativePath}");
                    }
                    var lines = File.ReadAllLines(file);
                    foreach (var line in lines)
                    {
                        if (removeEmptyLines && string.IsNullOrWhiteSpace(line)) continue;
                        writer.WriteLine(line);
                    }
                }
            }

            Console.WriteLine($"Bundle created successfully at {output.FullName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }, languageOption, fileOption, noteOption, sortOption, removeEmptyLinesOption, authorOption

);
var rootCommand = new RootCommand
{
    createRspCommand,
    bundleCommand
    
};

rootCommand.InvokeAsync(args);


