using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace OutfitGenerator_dotnetcore
{
    class ChestPantsMerger
    {
        private const int FRAME_SIZE = 43;
        private const int PANTS_WIDTH = 387;
        private const int PANTS_HEIGHT = 258;
        private const int PANTS_OLD_HEIGHT = 301;
        private const int CHEST_WIDTH = 86;
        private const int CHEST_HEIGHT = 258;

        private static Size chestSize = new Size(CHEST_WIDTH, CHEST_HEIGHT);
        private static Size pantsSize = new Size(PANTS_WIDTH, PANTS_HEIGHT);
        private static Size pantsOldSize = new Size(PANTS_WIDTH, PANTS_OLD_HEIGHT);

        public static void Generate(string[] args)
        {
            Image<Rgba32> result = null;

            if (args.Length != 2)
                Program.WaitAndExit("Improper usage! Expected parameters: <image_path_1> <image_path_2>\n" +
                            "Try dragging your image files directly on top of the application!");

            result = ChestAndPants(args[0], args[1]);
            string name = "mergedChestPants" + DateTime.Now.ToString(" MM.dd h.mm.ss") + ".png";

            DirectoryInfo directory = (new FileInfo(args[0])).Directory;
            string generatedFilePath = directory.FullName + "\\" + name;
            result.Save(generatedFilePath);
            Program.WaitAndExit("Done saving, check {0}", generatedFilePath);
            return;
        }

        private static Image<Rgba32> ChestAndPants(string firstPath, string secondPath)
        {
            Image<Rgba32> chestBitmap;
            Image<Rgba32> pantsBitmap;

            Console.WriteLine("Is the order correct?");
            Console.WriteLine("Chest image: " + firstPath);
            Console.WriteLine("Pants image: " + secondPath);
            Console.WriteLine("Press Enter if it is, press any key otherwise");

            using (FileStream firstStream = File.OpenRead(firstPath))
            using (FileStream secondStream = File.OpenRead(secondPath))
            {
                if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                {
                    chestBitmap = Image.Load<Rgba32>(firstStream);
                    pantsBitmap = Image.Load<Rgba32>(secondStream);
                }
                else
                {
                    chestBitmap = Image.Load<Rgba32>(secondStream);
                    pantsBitmap = Image.Load<Rgba32>(firstStream);
                }
            }

            if (!Generator.ValidSheet(chestBitmap, chestSize) || !Generator.ValidSheet(pantsBitmap, pantsSize, pantsOldSize))
            {
                Program.WaitAndExit("Incorrect size!\nExpected chest dimensions of {0}x{1} and pants dimensions of {2}x{3} or {4}x{5}.", 
                    chestSize.Width, chestSize.Height,
                    pantsSize.Width, pantsSize.Height,
                    pantsOldSize.Width, pantsOldSize.Height);
                return null;
            }

            return ApplyMultingChestPants(chestBitmap, pantsBitmap);
        }

        private static Image<Rgba32> ApplyMultingChestPants(Image<Rgba32> chest, Image<Rgba32> pants)
        {
            Image<Rgba32> result = pants.Clone();

            Image<Rgba32> chestIdle = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, 0, FRAME_SIZE, FRAME_SIZE)));
            Image<Rgba32> chestIdle2 = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(0, FRAME_SIZE, FRAME_SIZE, FRAME_SIZE)));
            Image<Rgba32> chestIdle3 = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, FRAME_SIZE, FRAME_SIZE, FRAME_SIZE)));

            Image<Rgba32> chestRun = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, FRAME_SIZE * 2, FRAME_SIZE, FRAME_SIZE)));

            Image<Rgba32> chestDuck = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, FRAME_SIZE * 3, FRAME_SIZE, FRAME_SIZE)));

            Image<Rgba32> chestClimb = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, FRAME_SIZE * 4, FRAME_SIZE, FRAME_SIZE)));
            Image<Rgba32> chestSwim = chest.Clone(x => x.Crop<Rgba32>(new Rectangle(FRAME_SIZE, FRAME_SIZE * 5, FRAME_SIZE, FRAME_SIZE)));

            // Personality 1,5
            Superimpose(result, chestIdle, FRAME_SIZE, 0);
            Superimpose(result, chestIdle, FRAME_SIZE * 5, 0);

            // Personality 2,4
            Superimpose(result, chestIdle2, FRAME_SIZE * 2, 0);
            Superimpose(result, chestIdle2, FRAME_SIZE * 4, 0);

            // Personality 3
            Superimpose(result, chestIdle3, FRAME_SIZE * 3, 0);

            // Duck
            Superimpose(result, chestDuck, 344, 0);

            // Sit
            Superimpose(result, chestIdle, 258, 1);

            // Walking
            Superimpose(result, chestIdle, FRAME_SIZE, FRAME_SIZE + 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 2, FRAME_SIZE + 2);
            Superimpose(result, chestIdle, FRAME_SIZE * 3, FRAME_SIZE + 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 4, FRAME_SIZE);
            Superimpose(result, chestIdle, FRAME_SIZE * 5, FRAME_SIZE + 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 6, FRAME_SIZE + 2);
            Superimpose(result, chestIdle, FRAME_SIZE * 7, FRAME_SIZE + 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 8, FRAME_SIZE);

            // Running
            Superimpose(result, chestRun, FRAME_SIZE, FRAME_SIZE * 2);
            Superimpose(result, chestRun, FRAME_SIZE * 2, FRAME_SIZE * 2 - 1);
            Superimpose(result, chestRun, FRAME_SIZE * 3, FRAME_SIZE * 2);
            Superimpose(result, chestRun, FRAME_SIZE * 4, FRAME_SIZE * 2 + 1);
            Superimpose(result, chestRun, FRAME_SIZE * 5, FRAME_SIZE * 2);
            Superimpose(result, chestRun, FRAME_SIZE * 6, FRAME_SIZE * 2 - 1);
            Superimpose(result, chestRun, FRAME_SIZE * 7, FRAME_SIZE * 2);
            Superimpose(result, chestRun, FRAME_SIZE * 8, FRAME_SIZE * 2 + 1);

            // Jumping
            Superimpose(result, chestIdle, FRAME_SIZE, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 2, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 3, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 4, FRAME_SIZE * 3 - 1);

            // Falling
            Superimpose(result, chestIdle, FRAME_SIZE * 5, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 6, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 7, FRAME_SIZE * 3 - 1);
            Superimpose(result, chestIdle, FRAME_SIZE * 8, FRAME_SIZE * 3 - 1);

            // Climbing
            Superimpose(result, chestClimb, FRAME_SIZE, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 2, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 3, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 4, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 5, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 6, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 7, FRAME_SIZE * 4);
            Superimpose(result, chestClimb, FRAME_SIZE * 8, FRAME_SIZE * 4);

            // Swimming
            Superimpose(result, chestSwim, FRAME_SIZE, FRAME_SIZE * 5);
            Superimpose(result, chestSwim, FRAME_SIZE * 4, FRAME_SIZE * 5);
            Superimpose(result, chestSwim, FRAME_SIZE * 5, FRAME_SIZE * 5 + 1);
            Superimpose(result, chestSwim, FRAME_SIZE * 6, FRAME_SIZE * 5 + 2);
            Superimpose(result, chestSwim, FRAME_SIZE * 7, FRAME_SIZE * 5 + 1);

            return result;
        }

        private static void Superimpose( Image<Rgba32> largeBmp, Image<Rgba32> smallBmp, int x, int y)
        {
            largeBmp.Mutate(pic => pic.DrawImage(smallBmp, PixelBlenderMode.Over, 1, new Point(x, y)));
        }
    }
}
