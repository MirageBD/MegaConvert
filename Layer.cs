using System;
using System.Collections.Generic;

namespace MegaConvert
{
    class ByteBuffer
    {
        public byte[] data;
        public int width;
        public int height;

        public ByteBuffer(int w, int h)
        {
            width = w;
            height = h;
            data = new byte[w * h];
        }
    }

    class HashCode
    {
        public ulong[] hashes = new ulong[8];

        public static HashCode FromByteBuffer(ByteBuffer bb)
        {
            var hash = new HashCode();

            for(int y = 0; y<8; y++)
            {
                ulong hashRow = 0;
                for (int x = 0; x < 8; x++)
                {
                    hashRow += ((ulong)bb.data[y * 8 + x]) << x;
                }
                hash.hashes[y] = hashRow;
            }

            return hash;
        }

        public static bool operator ==(HashCode a, HashCode b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HashCode a, HashCode b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object o)
        {
            if (o == null)
                return false;

            var second = o as HashCode;

            for (int i = 0; i < 8; i++)
            {
                if (hashes[i] != second.hashes[i])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public enum BitmapDirection
    {
        CharLeftRightTopBottom  = 0, // normal bitmap/char mode   - left to right first, top to bottom next, per char
        PixelTopBottomLeftRight = 1, // dma stretcher mode        - top to bottom first, left to right next, per pixel mode
        PixelLeftRightTopBottom = 2, // dma stretcher mode        - left to right first, top to bottom next, per pixel mode
        CharTopBottomLeftRight  = 3, // 'linear' bitmap/char mode - top to bottom first, left to right next, per char
        Other = 4,
    }

    public enum CharsetMode
    {
        Default = 0,
        SuperExtendedAttributeMode = 1,
        NibbleColour = 2
    }

    public enum SpriteMode
    {
        Default = 0,
        Colour16 = 1,
        Colour256 = 2
    }

    class Layer
    {
        public byte restrictmode;
        public ByteBuffer byteBuffer;
        public List<ByteBuffer> chars;
        public List<HashCode> hashes;
        public ByteBuffer screen;
        public ByteBuffer colours;
        public byte[] palRed = null;
        public byte[] palGreen = null;
        public byte[] palBlue = null;
        public int widthInChars;
        public int heightInChars;

        public Layer(int w, int h)
        {
            this.widthInChars = (w >> 3);
            this.heightInChars = (h >> 3);
            this.byteBuffer = new ByteBuffer(w, h);
            this.palRed = new byte[256];
            this.palGreen = new byte[256];
            this.palBlue = new byte[256];
        }

        public bool CharsetContainsChar(ByteBuffer bb)
        {
            for(int i=0; i<this.chars.Count; i++)
            {
                if (SameByteBuffer(bb, this.chars[i], 0, 0, 0, 0, 8, 8))
                    return true;
            }

            return false;
        }

        public void CompressChars()
        {
            int removed = 0;

            for (int i = chars.Count - 1; i >= 0; i--)
            {
                var hash1 = this.hashes[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    if (hash1 == this.hashes[j])
                    {
                        removed++;
                        this.chars.RemoveAt(i);
                        this.hashes.RemoveAt(i);
                        j = -1;
                    }
                }
            }

            Console.WriteLine("\nChars removed: " + removed);
        }

        public void ExtractChars(BitmapDirection direction, CharsetMode mode)
        {
            this.chars = new List<ByteBuffer>();
            this.hashes = new List<HashCode>();

            Console.WriteLine("Extracting chars in mode: " + mode);

            if (mode == CharsetMode.NibbleColour)
            {
                if (direction == BitmapDirection.CharLeftRightTopBottom)
                {
                    for (int row = 0; row < this.heightInChars; row++)
                    {
                        for (int column = 0; column < this.widthInChars; column += 2)
                        {
                            var charBuffer = new ByteBuffer(8, 8);
                            CopyByteBufferNibble(this.byteBuffer, charBuffer, column * 8, row * 8, 0, 0, 16, 8);

                            if (!CharsetContainsChar(charBuffer))
                            {
                                this.chars.Add(charBuffer);
                                this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                            }
                        }
                    }

                    Console.WriteLine("\nUnique chars: " + this.chars.Count);
                }
                else if (direction == BitmapDirection.PixelTopBottomLeftRight)
                {
                    for (int column = 0; column < this.widthInChars * 8; column += 2)
                    {
                        var charBuffer = new ByteBuffer(1, this.heightInChars * 8);
                        CopyByteBufferNibble(this.byteBuffer, charBuffer, column, 0, 0, 0, 2, this.heightInChars * 8);
                        this.chars.Add(charBuffer);
                        this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                    }
                }
            }
            else
            {
                if (direction == BitmapDirection.CharLeftRightTopBottom)
                {
                    for (int row = 0; row < this.heightInChars; row++)
                    {
                        for (int column = 0; column < this.widthInChars; column++)
                        {
                            var charBuffer = new ByteBuffer(8, 8);
                            CopyByteBuffer(this.byteBuffer, charBuffer, column * 8, row * 8, 0, 0, 8, 8);

                            this.chars.Add(charBuffer);
                            this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                        }
                    }

                    Console.WriteLine("\nUnique chars: " + this.chars.Count);
                }
                else if (direction == BitmapDirection.CharTopBottomLeftRight)
                {
                    for (int column = 0; column < this.widthInChars; column++)
                    {
                        for (int row = 0; row < this.heightInChars; row++)
                        {
                            var charBuffer = new ByteBuffer(8, 8);
                            CopyByteBuffer(this.byteBuffer, charBuffer, column * 8, row * 8, 0, 0, 8, 8);
                            this.chars.Add(charBuffer);
                            this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                        }
                    }
                }
                else if (direction == BitmapDirection.PixelTopBottomLeftRight)
                {
                    for (int column = 0; column < this.widthInChars * 8; column++)
                    {
                        var charBuffer = new ByteBuffer(1, this.heightInChars * 8);
                        CopyByteBuffer(this.byteBuffer, charBuffer, column, 0, 0, 0, 1, this.heightInChars * 8);
                        this.chars.Add(charBuffer);
                        this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                    }
                }
                else if (direction == BitmapDirection.PixelLeftRightTopBottom)
                {
                    for (int row = 0; row < this.heightInChars * 8; row++)
                    {
                        var charBuffer = new ByteBuffer(1, this.widthInChars * 8);
                        CopyByteBuffer(this.byteBuffer, charBuffer, 0, row, 0, 0, this.widthInChars * 8, 1);
                        this.chars.Add(charBuffer);
                        this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                    }
                }
                else if (direction == BitmapDirection.Other)
                {
                    for (int column = 0; column < this.widthInChars; column++)
                    {
                        for (int row = 0; row < this.heightInChars * 8; row++)
                        {
                            var charBuffer = new ByteBuffer(1, this.widthInChars * 8);
                            CopyByteBuffer(this.byteBuffer, charBuffer, 0, row, 0, 0, this.widthInChars * 8, 1);
                            this.chars.Add(charBuffer);
                            this.hashes.Add(HashCode.FromByteBuffer(charBuffer));
                        }
                    }
                }
            }
        }

        public void ExtractScreen(CharsetMode mode, UInt32 charLocation)
        {
            float charWidth = 1;
            if (mode == CharsetMode.SuperExtendedAttributeMode)
                charWidth = 2;
            else if (mode == CharsetMode.NibbleColour)
                charWidth = 1;

            this.screen = new ByteBuffer((int)(this.widthInChars * charWidth), this.heightInChars);

            if (mode == CharsetMode.NibbleColour)
            {
                for (int row = 0; row < this.heightInChars; row++)
                {
                    for (int column = 0; column < this.widthInChars; column += 2)
                    {
                        var charBuffer = new ByteBuffer(8, 8);
                        CopyByteBufferNibble(this.byteBuffer, charBuffer, column * 8, row * 8, 0, 0, 16, 8);

                        for (int i = 0; i < this.chars.Count; i++)
                        {
                            if (SameByteBuffer(charBuffer, this.chars[i], 0, 0, 0, 0, 8, 8))
                            {
                                int j = i + (int)(charLocation >> 6);
                                this.screen.data[(int)(row * this.widthInChars * charWidth + column * charWidth + 0)] = (byte)(j & 255);
                                this.screen.data[(int)(row * this.widthInChars * charWidth + column * charWidth + 1)] = (byte)(j >> 8);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int row = 0; row < this.heightInChars; row++)
                {
                    for (int column = 0; column < this.widthInChars; column++)
                    {
                        for (int i = 0; i < this.chars.Count; i++)
                        {
                            if (SameByteBuffer(this.byteBuffer, this.chars[i], column * 8, row * 8, 0, 0, 8, 8))
                            {
                                if (mode == CharsetMode.SuperExtendedAttributeMode)
                                {
                                    int j = i + (int)(charLocation>>6);
                                    this.screen.data[(int)(row * this.widthInChars * charWidth + column * charWidth + 0)] = (byte)(j & 255);
                                    this.screen.data[(int)(row * this.widthInChars * charWidth + column * charWidth + 1)] = (byte)(j >> 8);
                                }
                                else if (mode == CharsetMode.Default)
                                {
                                    this.screen.data[(int)(row * this.widthInChars * charWidth + column * charWidth + 0)] = (byte)(i);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ExtractAttributes()
        {
            this.colours = new ByteBuffer((int)(2 * this.widthInChars), this.heightInChars);

            for (int row = 0; row < this.heightInChars; row++)
            {
                for (int column = 0; column < this.widthInChars; column++)
                {
                    this.colours.data[(int)(row * 2 * this.widthInChars + column * 2 + 0)] = 0;

                    this.colours.data[(int)(row * 2 * this.widthInChars + column * 2 + 1)] =
                        this.byteBuffer.data[row * 64 * widthInChars + column * 8];
                }
            }
        }

        public static void CopyByteBuffer(ByteBuffer src, ByteBuffer dst, int srcx, int srcy, int dstx, int dsty, int w, int h)
        {
            for(int y = 0; y < h; y++)
            {
                for(int x = 0; x < w; x++)
                {
                    var srcByte = src.data[(y + srcy) * src.width + x + srcx];
                    dst.data[(y + dsty) * dst.width + x + dstx] = srcByte;
                }
            }
        }

        public static void CopyByteBufferNibble(ByteBuffer src, ByteBuffer dst, int srcx, int srcy, int dstx, int dsty, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w / 2; x++)
                {
                    var srcByte1 = src.data[(y + srcy) * src.width + srcx + 2 * x + 0];
                    var srcByte2 = src.data[(y + srcy) * src.width + srcx + 2 * x + 1];
                    var combined = (byte)(srcByte2 << 4 | srcByte1);
                    dst.data[(y + dsty) * dst.width + x + dstx] = combined;
                }
            }
        }

        public static bool SameByteBuffer(ByteBuffer src, ByteBuffer dst, int srcx, int srcy, int dstx, int dsty, int w, int h)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (dst.data[(y + dsty) * dst.width + x + dstx] != src.data[(y + srcy) * src.width + x + srcx])
                        return false;
                }
            }

            return true;
        }

    }
}
