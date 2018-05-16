using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Configuration;
using System.IO;

namespace OutfitGenerator_dotnetcore
{
    class BackGenerator
    {
        private static Size BACKITEM_SIZE = new Size(387, 301);

        public static void Generate(string[] args)
        {
            // Parameter checking
            if (args.Length != 1)
                Program.WaitAndExit("Improper usage! Expected parameter: <image_path>\n" +
                            "Try dragging your image file directly on top of the application!");

            // Image checking
            Image<Rgba32> target = null;

            try
            {
                using (FileStream stream = File.OpenRead(args[0]))
                {
                    target = Image.Load<Rgba32>(stream);
                }
            }
            catch (ArgumentException)
            {
                Program.WaitAndExit("The file \"{0}\" is not a valid image or does not exist.", args[0]);
                return;
            }

            // Begin
            Console.WriteLine("Starting generation.");
            Console.WriteLine("");

            string item;
            try
            {
                if (target == null)
                    throw new ArgumentNullException("Sheet may not be null.");

                if (!Generator.ValidSheet(target, BACKITEM_SIZE))
                    throw new GeneratorException($"Sheet dimensions must equal {BACKITEM_SIZE.Width}x{BACKITEM_SIZE.Height}, to match the back template.");

                
                item = Generator.Generate(target, Image.Load<Rgba32>(ResourceManager.GetResourceImage("Resources.animatedBackTemplate.png")), ResourceManager.GetResourceText("Resources.backTemplate.txt"));
            }
            catch (Exception exc)
            {
                Program.WaitAndExit(exc.Message);
                return;
            }

            DirectoryInfo directory = (new FileInfo(args[0])).Directory;

            // Save to disk
            string generatedFileName = Generator.Save(directory, item, "generatedBack");
            string generatedFilePath = directory + "\\" + generatedFileName;
            Console.WriteLine("Saved generated back item to {0}!", generatedFilePath);

            // Copy to clipboard
            //Clipboard.Copy(item);
            //Console.WriteLine("Copied command to clipboard!");
            Console.WriteLine("");

            Program.WaitAndExit("Done!");
        }
    }
}
