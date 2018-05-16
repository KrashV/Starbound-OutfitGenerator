using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.Primitives;
using System;
using System.IO;

namespace OutfitGenerator_dotnetcore
{
    class SleevesMerger
    {
        private const int SLEEVES_WIDTH = 387;
        private const int SLEEVES_HEIGHT = 301;
        private static Size sleevesSize = new Size(SLEEVES_WIDTH, SLEEVES_HEIGHT);

        public static void Generate(string[] args)
        {
            Image<Rgba32> result = null;

            if (args.Length != 2)
                Program.WaitAndExit("Improper usage! Expected parameters: <image_path_1> <image_path_2>\n" +
                            "Try dragging your image files directly on top of the application!");

            result = Sleeves(args[0], args[1]);

            string name = "mergedSleeves" + DateTime.Now.ToString(" MM.dd h.mm.ss") + ".png";
            DirectoryInfo directory = (new FileInfo(args[0])).Directory;
            string generatedFilePath = directory.FullName + "\\" + name;
            result.Save(generatedFilePath);
            Program.WaitAndExit("Done saving, check {0}", generatedFilePath);
            return;
        }

        private static Image<Rgba32> Sleeves(string firstPath, string secondPath)
        {
            Image<Rgba32> frontSleeves;
            Image<Rgba32> backSleeves;

            Console.WriteLine("Is the order correct?");
            Console.WriteLine("front sleeve image: " + firstPath);
            Console.WriteLine("back sleeve image: " + secondPath);
            Console.WriteLine("Press Enter if it is, press any key otherwise");

            try
            {
                using (FileStream firstStream = File.OpenRead(firstPath))
                using (FileStream secondStream = File.OpenRead(secondPath))
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                    {
                        frontSleeves = Image.Load<Rgba32>(firstStream);
                        backSleeves = Image.Load<Rgba32>(secondStream);
                    }
                    else
                    {
                        frontSleeves = Image.Load<Rgba32>(secondStream);
                        backSleeves = Image.Load<Rgba32>(firstStream);
                    }
                }
            }
            catch (ArgumentException)
            {
                Program.WaitAndExit($"The file \"{firstPath}\"  or \"{secondPath}\" is not a valid image or does not exist.");
                return null;
            }

            if (!Generator.ValidSheet(frontSleeves, sleevesSize) || !Generator.ValidSheet(frontSleeves, sleevesSize))
            {
                Program.WaitAndExit("Incorrect size!\nExpected sleeve dimensions of {0}x{1} for both files.",
                    sleevesSize.Width, sleevesSize.Height);
                return null;
            }

            return ApllyMultingSleeves(frontSleeves, backSleeves);
        }

        private static Image<Rgba32> ApllyMultingSleeves(Image<Rgba32> frontSleeves, Image<Rgba32> backSleeves)
        {
            Image<Rgba32> result = new Image<Rgba32>(SLEEVES_WIDTH, SLEEVES_HEIGHT * 2);

            Superimpose(result, frontSleeves, 0, 0);
            Superimpose(result, backSleeves, 0, SLEEVES_HEIGHT);

            return result;
        }

        private static void Superimpose(Image<Rgba32> largeBmp, Image<Rgba32> smallBmp, int x, int y)
        {
            largeBmp.Mutate(pic => pic.DrawImage(smallBmp, PixelBlenderMode.Over, 1, new Point(x, y)));
        }
    }
}
