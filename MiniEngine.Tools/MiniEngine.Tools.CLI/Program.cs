using System.CommandLine;
using MiniEngine.Tools.CLI;

var rootCommand = new RootCommand("MiniEngine Multi-Tool.");
rootCommand.SetHandler(Info.DoAction);

var packCommand = new Command("pack", "Packs game assets into the MEA format.");
var packPathArgument =
    new Argument<string>("dir", () => Environment.CurrentDirectory, "Asset root directory to pack.");
var packOutOption = new Option<string>(new[] {"output", "out", "o"}, () => "assets.mea", "Output filename.");
packCommand.AddArgument(packPathArgument);
packCommand.AddOption(packOutOption);
packCommand.SetHandler(Pack.DoAction, packPathArgument, packOutOption);
rootCommand.Add(packCommand);

await rootCommand.InvokeAsync(args);