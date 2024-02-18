using System;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MegaConvert
{
    class Program
    {
        // How to:    Open .pdn, export to raw.bin and on the export dialog set format to raw.
        // Then:      MegaConvert raw.bin

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: MegaConvert input.bin");
                Console.WriteLine("    d1:direction");
                Console.WriteLine("           0 = CharLeftRightTopBottom");
                Console.WriteLine("           1 = PixelTopBottomLeftRight");
                Console.WriteLine("           2 = PixelLeftRightTopBottom");
                Console.WriteLine("           3 = CharTopBottomLeftRight");
                Console.WriteLine("           4 = Other");
                Console.WriteLine("    cm1:character mode");
                Console.WriteLine("           0 = Default");
                Console.WriteLine("           1 = SuperExtendedAttributeMode");
                Console.WriteLine("           2 = NibbleColour");
                Console.WriteLine("    cl1:character location (hex)");
                Console.WriteLine("           E.G. cl1:2a000");
                Console.WriteLine("    rc1:reduce chars");
                Console.WriteLine("           0 = Don't reduce");
                Console.WriteLine("           1 = Reduce");
                Console.WriteLine("    sm1:sprite mode");
                Console.WriteLine("           0 = Default");
                Console.WriteLine("           1 = 256 colours");
                Console.WriteLine("           2 = 16 colours");
                return;
            }

            var inputFilename = args[0];

            var direction = args.SingleOrDefault(arg => arg.StartsWith("d1:"));
            var charmode = args.SingleOrDefault(arg => arg.StartsWith("cm1:"));
            var spritemode = args.SingleOrDefault(arg => arg.StartsWith("sm1:"));
            var charLocation = args.SingleOrDefault(arg => arg.StartsWith("cl1:"));
            var reducechars = args.SingleOrDefault(arg => arg.StartsWith("rc1:"));
            if (!string.IsNullOrEmpty(direction)) { direction = direction.Replace("d1:", ""); }
            if (!string.IsNullOrEmpty(charmode)) { charmode = charmode.Replace("cm1:", ""); }
            if (!string.IsNullOrEmpty(spritemode)) { spritemode = spritemode.Replace("sm1:", ""); }
            if (!string.IsNullOrEmpty(charLocation)) { charLocation = charLocation.Replace("cl1:", ""); }
            if (!string.IsNullOrEmpty(reducechars)) { reducechars = reducechars.Replace("rc1:", ""); }

            RawTimanthes rawTimanthes = new RawTimanthes();

            int.TryParse(direction, out var directionInt);

            if (directionInt == 0)
            {
                Console.WriteLine("\nSetting direction to CharLeftRightTopBottom");
                rawTimanthes.direction = BitmapDirection.CharLeftRightTopBottom;
            }
            else if (directionInt == 1)
            {
                Console.WriteLine("\nSetting direction to PixelTopBottomLeftRight");
                rawTimanthes.direction = BitmapDirection.PixelTopBottomLeftRight;
            }
            else if (directionInt == 2)
            {
                Console.WriteLine("\nSetting direction to PixelLeftRightTopBottom");
                rawTimanthes.direction = BitmapDirection.PixelLeftRightTopBottom;
            }
            else if (directionInt == 3)
            {
                Console.WriteLine("\nSetting direction to CharTopBottomLeftRight");
                rawTimanthes.direction = BitmapDirection.CharTopBottomLeftRight;
            }
            else if (directionInt == 4)
            {
                Console.WriteLine("\nSetting direction to CharTopBottomLeftRight");
                rawTimanthes.direction = BitmapDirection.Other;
            }
            else
            {
                Console.WriteLine("\nIllegal direction");
            }

            // bitmap.bin m1:1 d1: 2 cl1: 40000

            int.TryParse(charmode, out var charModeInt);

            if(charModeInt == 0)
            {
                Console.WriteLine("\nSetting charsetMode to Default");
                rawTimanthes.charsetMode = CharsetMode.Default;
            }
            else if(charModeInt == 1)
            {
                Console.WriteLine("\nSetting charsetMode to SuperExtendedAttributeMode");
                rawTimanthes.charsetMode = CharsetMode.SuperExtendedAttributeMode;
            }
            else if(charModeInt == 2)
            {
                Console.WriteLine("\nSetting charsetMode to NibbleColour");
                rawTimanthes.charsetMode = CharsetMode.NibbleColour;
            }

            int.TryParse(spritemode, out var spriteModeInt);

            if (spriteModeInt == 0)
            {
                rawTimanthes.spriteMode =SpriteMode.Default;
            }
            else if (spriteModeInt == 1)
            {
                rawTimanthes.spriteMode = SpriteMode.Colour256;
            }
            else if (spriteModeInt == 2)
            {
                rawTimanthes.spriteMode = SpriteMode.Colour16;
            }

            UInt32.TryParse(charLocation, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var charLocationInt);
            rawTimanthes.charLocation = charLocationInt;

            UInt32.TryParse(reducechars, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var reducecharsInt);
            rawTimanthes.reduceChars = reducecharsInt > 0 ? true : false;

            Console.WriteLine("\nSetting reduceChars to " + rawTimanthes.reduceChars);

            rawTimanthes.ReadFile(inputFilename);

            Parser parser = new Parser();
            parser.fileName = Path.GetFileNameWithoutExtension(inputFilename);
            parser.filePath = Path.GetDirectoryName(inputFilename);

            parser.Parse(rawTimanthes);
            parser.WriteFiles();
        }
    }
}
