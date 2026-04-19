using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MegaConvert
{
	class Parser
	{
		public  RawTimanthes					rawTimanthes            = null;

		public string fileName;
		public string filePath;
        public string outputFilePath;

        public Parser()
		{
		}

		public void Parse(RawTimanthes raw)
		{
			rawTimanthes = raw;
		}

		public static byte ReverseNibble(byte b)
        {
			return (byte)((b >> 4) | ((b & 15) << 4));
        }

		public void WriteFiles()
		{
			FileStream file;

			Console.WriteLine("\nWriting files: ");

            string fn = this.fileName;
			string fp = this.filePath;

            if (!string.IsNullOrEmpty(this.outputFilePath))
                fp = this.outputFilePath;

            if (!String.IsNullOrEmpty(fp))
            {
                Console.WriteLine("INPUT FILEPATH: " + fp);
            }

            if (!String.IsNullOrEmpty(fn))
            {
                Console.WriteLine("INPUT FILENAME: " + fn);
            }

            for (int i = 0; i < rawTimanthes.layers.Length; i++)
			{
				var layer = rawTimanthes.layers[i];

                Console.WriteLine("layer.restrictmode: " + layer.restrictmode);

                if (layer.restrictmode == 0x02) // bitmap multicolour
                {
                    /*
                    var fn2 = fp + "//" + fn + "_chars" + i + ".bin";
                    Console.WriteLine(fn2);
                    File.Delete(fn2);
                    file = File.OpenWrite(fn2);
                    var chars = rawTimanthes.layers[i].chars;
                    foreach (var c in chars)
                    {
                        for (int y = 0; y < c.height; y++)
                        {
                            for (int x = 0; x < c.width; x++)
                            {
                                file.WriteByte((byte)(c.data[y * c.width + x]));
                            }
                        }
                    }
                    file.Close();
                    */

                    var fn2 = fp + "//" + fn + "_sprites" + i + ".bin";
                    Console.WriteLine(fn2);
                    File.Delete(fn2);
                    file = File.OpenWrite(fn2);

                    var bb = rawTimanthes.layers[i].byteBuffer;

                    if (rawTimanthes.spriteMode == SpriteMode.Colour3_64wide) // assumes hires multicolour layer
                    {
                        for (int spr = 0; spr < bb.width / 64; spr++)
                        {
                            for (int y = 0; y < bb.height; y++)
                            {
                                for (int x = 0; x < 64; x += 8)
                                {
                                    byte b0 = (byte)(bb.data[y * bb.width + (spr * 64) + x + 0] + 1);
                                    byte b1 = (byte)(bb.data[y * bb.width + (spr * 64) + x + 2] + 1);
                                    byte b2 = (byte)(bb.data[y * bb.width + (spr * 64) + x + 4] + 1);
                                    byte b3 = (byte)(bb.data[y * bb.width + (spr * 64) + x + 6] + 1);

                                    if (b0 > 3) b0 = 0; // $10 ($11 after I added 1) is transparent, but assume everything above 3 is transparent
                                    if (b1 > 3) b1 = 0;
                                    if (b2 > 3) b2 = 0;
                                    if (b3 > 3) b3 = 0;

                                    byte b = (byte)((b0 << 6) + (b1 << 4) + (b2 << 2) + (b3 << 0));
                                    file.WriteByte(b);
                                }
                            }
                        }
                    }

                    file.Close();
                }
                else if (layer.restrictmode == 0x03) // bitmap singlecolour/singlecolor
                {
                    var fn2 = fp + "//" + fn + "_chars" + i + ".bin";
                    Console.WriteLine(fn2);
                    File.Delete(fn2);
                    file = File.OpenWrite(fn2);
                    var chars = rawTimanthes.layers[i].chars;
                    foreach (var c in chars)
                    {
                        for (int y = 0; y < c.height; y++)
                        {
                            for (int x = 0; x < c.width; x++)
                            {
                                file.WriteByte((byte)(c.data[y * c.width + x]));
                            }
                        }
                    }
                    file.Close();

                    fn2 = fp + "//" + fn + "_sprites" + i + ".bin";
                    Console.WriteLine(fn2);
                    File.Delete(fn2);
                    file = File.OpenWrite(fn2);

                    var bb = rawTimanthes.layers[i].byteBuffer;

                    if (rawTimanthes.spriteMode == SpriteMode.Colour1_64wide) // assumes hires singlecolour layer
                    {
                        for (int spr = 0; spr < bb.width / 64; spr++)
                        {
                            for (int y = 0; y < bb.height; y++)
                            {
                                for (int x = 0; x < 64; x += 8)
                                {
                                    byte b = (byte)(
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 0]) << 7) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 1]) << 6) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 2]) << 5) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 3]) << 4) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 4]) << 3) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 5]) << 2) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 6]) << 1) +
                                                 ((bb.data[y * bb.width + (spr * 64) + x + 7]) << 0)
                                             );
                                    file.WriteByte(b);
                                }
                            }
                        }
                    }

                    file.Close();
                }
                else if (layer.restrictmode == 0x17) // attribute/colour?
                {
                    var fn2 = fp + "//" + fn + "_cols" + i + ".bin";
                    Console.WriteLine(fn2);
                    File.Delete(fn2);
                    file = File.OpenWrite(fn2);
                    var cols = rawTimanthes.layers[i].colours;
                    for (int x = 0; x < cols.data.Length; x++)
                    {
                        file.WriteByte((byte)(cols.data[x]));
                    }
                    file.Close();
                }
                else if (layer.restrictmode == 0x09 || layer.restrictmode == 0x0a || layer.restrictmode == 0x8a) // FCM or NCM or NCM512
                {
                    if (layer.restrictmode == 0x09 && rawTimanthes.charsetMode == CharsetMode.Bitplane) // extract bitplanes
                    {
                        for (int bp = 0; bp < 8; bp++) // 8 bitplanes
                        {
                            byte mask = (byte)(1 << bp); // 1,2,4,8,16,32,64,128

                            // bitplane 0 masks out all the 1s
                            // bitplane 1 masks out all the 2s
                            // bitplane 2 masks out all the 4s etc.

                            var fn2 = fp + "//" + fn + "_bitplane" + i + bp + ".bin";
                            Console.WriteLine(fn2);
                            File.Delete(fn2);
                            file = File.OpenWrite(fn2);
                            var chars = rawTimanthes.layers[i].chars;
                            foreach (var c in chars)
                            {
                                for (int y = 0; y < c.height; y++)
                                {
                                    byte b = 0;
                                    for (int x = 0; x < c.width; x++)
                                    {
                                        byte t = c.data[y * c.width + x]; // palette index 0-255
                                        t &= mask;
                                        b += (byte)((t >> bp) << (7-x));
                                    }
                                    file.WriteByte((byte)b);
                                }
                            }
                            file.Close();
                        }
                    }
                    else
                    {
                        var fn2 = fp + "//" + fn + "_chars" + i + ".bin";
                        Console.WriteLine(fn2);
                        File.Delete(fn2);
                        file = File.OpenWrite(fn2);
                        var chars = rawTimanthes.layers[i].chars;
                        foreach (var c in chars)
                        {
                            for (int y = 0; y < c.height; y++)
                            {
                                for (int x = 0; x < c.width; x++)
                                {
                                    file.WriteByte((byte)(c.data[y * c.width + x]));
                                }
                            }
                        }
                        file.Close();

                        fn2 = fp + "//" + fn + "_screen" + i + ".bin";
                        Console.WriteLine(fn2);
                        File.Delete(fn2);
                        file = File.OpenWrite(fn2);
                        var screen = rawTimanthes.layers[i].screen;
                        for (int x = 0; x < screen.data.Length; x++)
                        {
                            file.WriteByte((byte)(screen.data[x]));
                        }
                        file.Close();

                        if (layer.restrictmode == 0x0a || layer.restrictmode == 0x8a) // 0x8a = 512 colour nybble mode
                        {
                            fn2 = fp + "//" + fn + "_attrib" + i + ".bin";
                            Console.WriteLine(fn2);
                            File.Delete(fn2);
                            file = File.OpenWrite(fn2);
                            var colours = rawTimanthes.layers[i].colours;
                            for (int x = 0; x < colours.data.Length; x++)
                            {
                                file.WriteByte((byte)(colours.data[x]));
                            }
                            file.Close();
                        }

                        fn2 = fp + "//" + fn + "_pal" + i + ".bin";
                        Console.WriteLine(fn2);
                        File.Delete(fn2);
                        file = File.OpenWrite(fn2);
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.palRed[x]));
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.palGreen[x]));
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.palBlue[x]));
                        file.Close();

                        fn2 = fp + "//" + fn + "_pal2" + i + ".bin";
                        Console.WriteLine(fn2);
                        File.Delete(fn2);
                        file = File.OpenWrite(fn2);
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.pal2Red[x]));
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.pal2Green[x]));
                        for (int x = 0; x < 256; x++)
                            file.WriteByte(ReverseNibble(layer.pal2Blue[x]));
                        file.Close();

                        fn2 = fp + "//" + fn + "_sprites" + i + ".bin";
                        Console.WriteLine(fn2);
                        File.Delete(fn2);
                        file = File.OpenWrite(fn2);

                        // for FCM/NCM layers, always extract sprites

                        var bb = rawTimanthes.layers[i].byteBuffer;

                        if (rawTimanthes.spriteMode == SpriteMode.Colour16_64wide) // assumes NCM layer. 4 bits per pixel - 64/4 = 16 pixels wide
                        {
                            for (int spr = 0; spr < bb.width / 16; spr++)
                            {
                                for (int y = 0; y < bb.height; y++)
                                {
                                    for (int x = 0; x < 16; x += 2)
                                    {
                                        byte b = (byte)(
                                                     ((bb.data[y * bb.width + (spr * 16) + x + 0]) << 4) +
                                                     ((bb.data[y * bb.width + (spr * 16) + x + 1]) << 0)
                                                 );
                                        file.WriteByte(b);
                                    }
                                }
                            }
                        }

                        file.Close();
                    }
                }
                else
                {
                    Console.WriteLine("ERROR - UNRECOGNIZED RESTRICT MODE: " + layer.restrictmode);
                }
            }

			/*
			fn = this.outputSprite; // "spr.bin";
			if (!String.IsNullOrEmpty(fn))
			{
				Console.WriteLine(fn);
				File.Delete(fn);
				file = File.OpenWrite(fn);
				for (int i = 0; i < rawTimanthes.layers.Length; i++)
				{
					var bb = rawTimanthes.layers[i].byteBuffer;

					if (rawTimanthes.spriteMode == SpriteMode.Colour256)
					{
						for (int spr = 0; spr < bb.width / 16; spr++)
						{
							for (int y = 0; y < bb.height; y++)
							{
								for (int x = 0; x < 16; x += 2)
								{
									byte b = (byte)(((bb.data[y * bb.width + (spr * 16) + x + 0]) << 4) + ((bb.data[y * bb.width + (spr * 16) + x + 1]) << 0));
									file.WriteByte(b);
								}
							}
						}
					}
					else if(rawTimanthes.spriteMode == SpriteMode.Colour16)
                    {
						for (int spr = 0; spr < bb.width / 32; spr++)
						{
							for (int y = 0; y < bb.height; y++)
							{
								for (int x = 0; x < 32; x ++)
								{
									byte b = (byte)(bb.data[y * bb.width + (spr * 32) + x]);
									file.WriteByte(b);
								}
							}
						}
					}

				}
				file.Close();
			}
			*/
		}
	}
}
