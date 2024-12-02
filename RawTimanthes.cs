using System;
using System.IO;
using System.Reflection.Emit;

namespace MegaConvert
{
    class RawTimanthes
    {
        public Layer[] layers = null;             // original layers, 1 byte for each color palette entry
        public BitmapDirection direction;
        public CharsetMode charsetMode;
        public SpriteMode spriteMode;
        public UInt32 charLocation;
        public bool reduceChars;

        public RawTimanthes()
        {
        }

        public void ReadFile(string filename)
        {
            int headerSize = 5;
            int paletteSize = 256;
            byte[] fileBytes = File.ReadAllBytes(filename);

            int width = (UInt16)(((UInt16)(fileBytes[0]) << 8) + (UInt16)(fileBytes[1]));
            int height = (UInt16)(((UInt16)(fileBytes[2]) << 8) + (UInt16)(fileBytes[3]));
            int numLayers = (UInt16)(fileBytes[4]);

            // store all the layers in an array
            this.layers = new Layer[numLayers];

            for (int layer = 0; layer < numLayers; layer++)
            {
                this.layers[layer] = new Layer(width, height);
                this.layers[layer].restrictmode = (byte)(fileBytes[headerSize++]);
            }

            Console.WriteLine("WIDTH: {0} - HEIGHT: {1} - LAYERS: {2}", width, height, numLayers);

            int walker = headerSize;

            for (int layer = 0; layer < numLayers; layer++)
            {
                if (this.charsetMode == CharsetMode.NibbleColour512)
                {
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palRed[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palGreen[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palBlue[i] = fileBytes[walker++];

                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].pal2Red[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].pal2Green[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].pal2Blue[i] = fileBytes[walker++];

                    for (int offset = 0; offset < (width * height); offset++)
                    {
                        this.layers[layer].byteBufferHi.data[offset] = fileBytes[walker++];
                        this.layers[layer].byteBuffer.data[offset] = fileBytes[walker++];
                    }

                    this.layers[layer].ExtractChars(direction, charsetMode);

                    if (reduceChars)
                        this.layers[layer].CompressChars();

                    if (reduceChars)
                        this.layers[layer].ExtractScreen(charsetMode, charLocation);
                    else
                        this.layers[layer].ConstructScreen(charsetMode, charLocation);

                    this.layers[layer].ExtractAttributes512();
                }
                else
                {
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palRed[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palGreen[i] = fileBytes[walker++];
                    for (int i = 0; i < paletteSize; i++)
                        this.layers[layer].palBlue[i] = fileBytes[walker++];

                    for (int offset = 0; offset < (width * height); offset++)
                        this.layers[layer].byteBuffer.data[offset] = fileBytes[walker++];

                    this.layers[layer].ExtractChars(direction, charsetMode);

                    if (reduceChars)
                        this.layers[layer].CompressChars();

                    if (reduceChars)
                        this.layers[layer].ExtractScreen(charsetMode, charLocation);
                    else
                        this.layers[layer].ConstructScreen(charsetMode, charLocation);

                    this.layers[layer].ExtractAttributes();
                }
            }
        }
    }
}
