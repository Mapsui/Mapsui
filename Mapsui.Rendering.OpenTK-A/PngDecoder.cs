/*
 * Copyright (c) 2008-2010, Matthias Mann
 * 
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following
 * conditions are met:
 * 
 * * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following
 * disclaimer in the documentation and/or other materials provided with the distribution.
 * 
 * * Neither the name of Matthias Mann nor the names of its contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING,
 * BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */


using Java.IO;
using Java.Lang;
using Java.Nio;
using Java.Nio.Extensions;
using Java.Util;
using Java.Util.Zip;

namespace com.badlogic.gdx.backends.angle
{


    /**
     * Decodes A PNG directly a ByteBuffer.
     * 
     * @author Matthias Mann
     * @author Nathan Sweet <misc@n4te.com> (minor edits to avoid allocation)
     */
    class PNGDecoder
    {

        public enum Format
        {
            ALPHA,
            LUMINANCE,
            LUMINANCE_ALPHA,
            RGB,
            RGBA,
            BGRA,
            ABGR
        };

        //int numComponents;
        //    bool hasAlpha;

        //    private Format (int numComponents, bool hasAlpha) {
        //        this.numComponents = numComponents;
        //        this.hasAlpha = hasAlpha;
        //    }

        //    public int getNumComponents () {
        //        return numComponents;
        //    }

        //    public bool isHasAlpha () {
        //        return hasAlpha;
        //    }
        //}

        private static byte[] SIGNATURE = { 137, 80, 78, 71, 13, 10, 26, 10 };

        private const int IHDR = 0x49484452;
        private const int PLTE = 0x504C5445;
        private const int tRNS = 0x74524E53;
        private const int IDAT = 0x49444154;
        private const int IEND = 0x49454E44;

        private const byte COLOR_GREYSCALE = 0;
        private const byte COLOR_TRUECOLOR = 2;
        private const byte COLOR_INDEXED = 3;
        private const byte COLOR_GREYALPHA = 4;
        private const byte COLOR_TRUEALPHA = 6;

        private InputStream input;
        private CRC32 crc;
        private byte[] buffer;

        private int chunkLength;
        private int chunkType;
        private int chunkRemaining;

        private int width;
        private int height;
        private int bitdepth;
        private int colorType;
        private int bytesPerPixel;
        private byte[] palette;
        private byte[] paletteA;
        private byte[] transPixel;

        public PNGDecoder()
        {
            this.crc = new CRC32();
            this.buffer = new byte[4096];
        }

        public int getHeight()
        {
            return height;
        }

        public int getWidth()
        {
            return width;
        }

        public bool hasAlpha()
        {
            return colorType == COLOR_TRUEALPHA || paletteA != null || transPixel != null;
        }

        public bool isRGB()
        {
            return colorType == COLOR_TRUEALPHA || colorType == COLOR_TRUECOLOR || colorType == COLOR_INDEXED;
        }

        /**
         * Computes the implemented format conversion for the desired format.
         * 
         * @param fmt the desired format
         * @return format which best matches the desired format
         * @throws UnsupportedOperationException if this PNG file can't be decoded
         */
        public Format decideTextureFormat(Format fmt)
        {
            switch (colorType)
            {
                case COLOR_TRUECOLOR:
                    switch (fmt)
                    {
                        case Format.ABGR:
                        case Format.RGBA:
                        case Format.BGRA:
                        case Format.RGB:
                            return fmt;
                        default:
                            return Format.RGB;
                    }
                case COLOR_TRUEALPHA:
                    switch (fmt)
                    {
                        case Format.ABGR:
                        case Format.RGBA:
                        case Format.BGRA:
                        case Format.RGB:
                            return fmt;
                        default:
                            return Format.RGBA;
                    }
                case COLOR_GREYSCALE:
                    switch (fmt)
                    {
                        case Format.LUMINANCE:
                        case Format.ALPHA:
                            return fmt;
                        default:
                            return Format.LUMINANCE;
                    }
                case COLOR_GREYALPHA:
                    return Format.LUMINANCE_ALPHA;
                case COLOR_INDEXED:
                    switch (fmt)
                    {
                        case Format.ABGR:
                        case Format.RGBA:
                        case Format.BGRA:
                            return fmt;
                        default:
                            return Format.RGBA;
                    }
                default:
                    throw new UnsupportedOperationException("Not yet implemented");
            }
        }

        public void decode(ByteBuffer buffer, int stride, Format fmt)
        {
            int offset = buffer.Position();
            int lineSize = ((width * bitdepth + 7) / 8) * bytesPerPixel;
            byte[] curLine = new byte[lineSize + 1];
            byte[] prevLine = new byte[lineSize + 1];
            byte[] palLine = (bitdepth < 8) ? new byte[width + 1] : null;

            Inflater inflater = new Inflater();
            try
            {
                for (int y = 0; y < height; y++)
                {
                    readChunkUnzip(inflater, curLine, 0, curLine.Length);
                    unfilter(curLine, prevLine);

                    buffer.Position(offset + y * stride);

                    switch (colorType)
                    {
                        case COLOR_TRUECOLOR:
                            switch (fmt)
                            {
                                case Format.ABGR:
                                    copyRGBtoABGR(buffer, curLine);
                                    break;
                                case Format.RGBA:
                                    copyRGBtoRGBA(buffer, curLine);
                                    break;
                                case Format.BGRA:
                                    copyRGBtoBGRA(buffer, curLine);
                                    break;
                                case Format.RGB:
                                    copy(buffer, curLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported format for this image");
                            }
                            break;
                        case COLOR_TRUEALPHA:
                            switch (fmt)
                            {
                                case Format.ABGR:
                                    copyRGBAtoABGR(buffer, curLine);
                                    break;
                                case Format.RGBA:
                                    copy(buffer, curLine);
                                    break;
                                case Format.BGRA:
                                    copyRGBAtoBGRA(buffer, curLine);
                                    break;
                                case Format.RGB:
                                    copyRGBAtoRGB(buffer, curLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported format for this image");
                            }
                            break;
                        case COLOR_GREYSCALE:
                            switch (fmt)
                            {
                                case Format.LUMINANCE:
                                case Format.ALPHA:
                                    copy(buffer, curLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported format for this image");
                            }
                            break;
                        case COLOR_GREYALPHA:
                            switch (fmt)
                            {
                                case Format.LUMINANCE_ALPHA:
                                    copy(buffer, curLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported format for this image");
                            }
                            break;
                        case COLOR_INDEXED:
                            switch (bitdepth)
                            {
                                case 8:
                                    palLine = curLine;
                                    break;
                                case 4:
                                    expand4(curLine, palLine);
                                    break;
                                case 2:
                                    expand2(curLine, palLine);
                                    break;
                                case 1:
                                    expand1(curLine, palLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported bitdepth for this image");
                            }
                            switch (fmt)
                            {
                                case Format.ABGR:
                                    copyPALtoABGR(buffer, palLine);
                                    break;
                                case Format.RGBA:
                                    copyPALtoRGBA(buffer, palLine);
                                    break;
                                case Format.BGRA:
                                    copyPALtoBGRA(buffer, palLine);
                                    break;
                                default:
                                    throw new UnsupportedOperationException("Unsupported format for this image");
                            }
                            break;
                        default:
                            throw new UnsupportedOperationException("Not yet implemented");
                    }

                    byte[] tmp = curLine;
                    curLine = prevLine;
                    prevLine = tmp;
                }
            }
            finally
            {
                inflater.End();
            }
        }

        private void copy(ByteBuffer buffer, byte[] curLine)
        {
            buffer.Put(curLine, 1, curLine.Length - 1);
        }

        private void copyRGBtoABGR(ByteBuffer buffer, byte[] curLine)
        {
            if (transPixel != null)
            {
                byte tr = transPixel[1];
                byte tg = transPixel[3];
                byte tb = transPixel[5];
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    byte r = curLine[i];
                    byte g = curLine[i + 1];
                    byte b = curLine[i + 2];
                    byte a = 0xFF;
                    if (r == tr && g == tg && b == tb)
                    {
                        a = 0;
                    }
                    buffer.Put(a).Put(b).Put(g).Put(r);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    buffer.Put((byte)0xFF).Put(curLine[i + 2]).Put(curLine[i + 1]).Put(curLine[i]);
                }
            }
        }

        private void copyRGBtoRGBA(ByteBuffer buffer, byte[] curLine)
        {
            if (transPixel != null)
            {
                byte tr = transPixel[1];
                byte tg = transPixel[3];
                byte tb = transPixel[5];
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    byte r = curLine[i];
                    byte g = curLine[i + 1];
                    byte b = curLine[i + 2];
                    byte a = (byte)0xFF;
                    if (r == tr && g == tg && b == tb)
                    {
                        a = 0;
                    }
                    buffer.Put(r).Put(g).Put(b).Put(a);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    buffer.Put(curLine[i]).Put(curLine[i + 1]).Put(curLine[i + 2]).Put((byte)0xFF);
                }
            }
        }

        private void copyRGBtoBGRA(ByteBuffer buffer, byte[] curLine)
        {
            if (transPixel != null)
            {
                byte tr = transPixel[1];
                byte tg = transPixel[3];
                byte tb = transPixel[5];
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    byte r = curLine[i];
                    byte g = curLine[i + 1];
                    byte b = curLine[i + 2];
                    byte a = (byte)0xFF;
                    if (r == tr && g == tg && b == tb)
                    {
                        a = 0;
                    }
                    buffer.Put(b).Put(g).Put(r).Put(a);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 3)
                {
                    buffer.Put(curLine[i + 2]).Put(curLine[i + 1]).Put(curLine[i]).Put((byte)0xFF);
                }
            }
        }

        private void copyRGBAtoABGR(ByteBuffer buffer, byte[] curLine)
        {
            for (int i = 1, n = curLine.Length; i < n; i += 4)
            {
                buffer.Put(curLine[i + 3]).Put(curLine[i + 2]).Put(curLine[i + 1]).Put(curLine[i]);
            }
        }

        private void copyRGBAtoBGRA(ByteBuffer buffer, byte[] curLine)
        {
            for (int i = 1, n = curLine.Length; i < n; i += 4)
            {
                buffer.Put(curLine[i + 2]).Put(curLine[i + 1]).Put(curLine[i + 0]).Put(curLine[i + 3]);
            }
        }

        private void copyRGBAtoRGB(ByteBuffer buffer, byte[] curLine)
        {
            for (int i = 1, n = curLine.Length; i < n; i += 4)
            {
                buffer.Put(curLine[i]).Put(curLine[i + 1]).Put(curLine[i + 2]);
            }
        }

        private void copyPALtoABGR(ByteBuffer buffer, byte[] curLine)
        {
            if (paletteA != null)
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = paletteA[idx];
                    buffer.Put(a).Put(b).Put(g).Put(r);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = (byte)0xFF;
                    buffer.Put(a).Put(b).Put(g).Put(r);
                }
            }
        }

        private void copyPALtoRGBA(ByteBuffer buffer, byte[] curLine)
        {
            if (paletteA != null)
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = paletteA[idx];
                    buffer.Put(r).Put(g).Put(b).Put(a);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = (byte)0xFF;
                    buffer.Put(r).Put(g).Put(b).Put(a);
                }
            }
        }

        private void copyPALtoBGRA(ByteBuffer buffer, byte[] curLine)
        {
            if (paletteA != null)
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = paletteA[idx];
                    buffer.Put(b).Put(g).Put(r).Put(a);
                }
            }
            else
            {
                for (int i = 1, n = curLine.Length; i < n; i += 1)
                {
                    int idx = curLine[i] & 255;
                    byte r = palette[idx * 3 + 0];
                    byte g = palette[idx * 3 + 1];
                    byte b = palette[idx * 3 + 2];
                    byte a = 0xFF;
                    buffer.Put(b).Put(g).Put(r).Put(a);
                }
            }
        }

        private void expand4(byte[] src, byte[] dst)
        {
            for (int i = 1, n = dst.Length; i < n; i += 2)
            {
                int val = src[1 + (i >> 1)] & 255;
                switch (n - i)
                {
                    default:
                        dst[i + 1] = (byte)(val & 15);
                        break;
                    case 1:
                        dst[i] = (byte)(val >> 4);
                        break;
                }
            }
        }

        private void expand2(byte[] src, byte[] dst)
        {
            for (int i = 1, n = dst.Length; i < n; i += 4)
            {
                int val = src[1 + (i >> 2)] & 255;
                switch (n - i)
                {
                    default:
                        dst[i + 3] = (byte)((val) & 3);
                        break;
                    case 3:
                        dst[i + 2] = (byte)((val >> 2) & 3);
                        break;
                    case 2:
                        dst[i + 1] = (byte)((val >> 4) & 3);
                        break;
                    case 1:
                        dst[i] = (byte)((val >> 6));
                        break;
                }
            }
        }

        private void expand1(byte[] src, byte[] dst)
        {
            for (int i = 1, n = dst.Length; i < n; i += 8)
            {
                int val = src[1 + (i >> 3)] & 255;
                switch (n - i)
                {
                    default:
                        dst[i + 7] = (byte)((val) & 1); break;
                    case 7:
                        dst[i + 6] = (byte)((val >> 1) & 1); break;
                    case 6:
                        dst[i + 5] = (byte)((val >> 2) & 1); break;
                    case 5:
                        dst[i + 4] = (byte)((val >> 3) & 1); break;
                    case 4:
                        dst[i + 3] = (byte)((val >> 4) & 1); break;
                    case 3:
                        dst[i + 2] = (byte)((val >> 5) & 1); break;
                    case 2:
                        dst[i + 1] = (byte)((val >> 6) & 1); break;
                    case 1:
                        dst[i] = (byte)((val >> 7));
                        break;
                }
            }
        }

        private void unfilter(byte[] curLine, byte[] prevLine)
        {
            switch (curLine[0])
            {
                case 0: // none
                    break;
                case 1:
                    unfilterSub(curLine);
                    break;
                case 2:
                    unfilterUp(curLine, prevLine);
                    break;
                case 3:
                    unfilterAverage(curLine, prevLine);
                    break;
                case 4:
                    unfilterPaeth(curLine, prevLine);
                    break;
                default:
                    throw new IOException("invalide filter type in scanline: " + curLine[0]);
            }
        }

        private void unfilterSub(byte[] curLine)
        {
            int bpp = this.bytesPerPixel;
            for (int i = bpp + 1, n = curLine.Length; i < n; ++i)
            {
                curLine[i] += curLine[i - bpp];
            }
        }

        private void unfilterUp(byte[] curLine, byte[] prevLine)
        {
            for (int i = 1, n = curLine.Length; i < n; ++i)
            {
                curLine[i] += prevLine[i];
            }
        }

        private void unfilterAverage(byte[] curLine, byte[] prevLine)
        {
            int bpp = this.bytesPerPixel;

            int i;
            for (i = 1; i <= bpp; ++i)
            {
                curLine[i] += (byte)((prevLine[i] & 0xFF) >> 1);
            }
            for (int n = curLine.Length; i < n; ++i)
            {
                curLine[i] += (byte)(((prevLine[i] & 0xFF) + (curLine[i - bpp] & 0xFF)) >> 1);
            }
        }

        private void unfilterPaeth(byte[] curLine, byte[] prevLine)
        {
            int bpp = bytesPerPixel;

            int i;
            for (i = 1; i <= bpp; ++i)
            {
                curLine[i] += prevLine[i];
            }
            for (int n = curLine.Length; i < n; ++i)
            {
                int a = curLine[i - bpp] & 255;
                int b = prevLine[i] & 255;
                int c = prevLine[i - bpp] & 255;
                int p = a + b - c;
                int pa = p - a;
                if (pa < 0) pa = -pa;
                int pb = p - b;
                if (pb < 0) pb = -pb;
                int pc = p - c;
                if (pc < 0) pc = -pc;
                if (pa <= pb && pa <= pc)
                    c = a;
                else if (pb <= pc) c = b;
                curLine[i] += (byte)c;
            }
        }

        private void readIHDR()
        {
            checkChunkLength(13);
            readChunk(buffer, 0, 13);
            width = readInt(buffer, 0);
            height = readInt(buffer, 4);
            bitdepth = buffer[8] & 255;
            colorType = buffer[9] & 255;

            switch (colorType)
            {
                case COLOR_GREYSCALE:
                    if (bitdepth != 8)
                    {
                        throw new IOException("Unsupported bit depth: " + bitdepth);
                    }
                    bytesPerPixel = 1;
                    break;
                case COLOR_GREYALPHA:
                    if (bitdepth != 8)
                    {
                        throw new IOException("Unsupported bit depth: " + bitdepth);
                    }
                    bytesPerPixel = 2;
                    break;
                case COLOR_TRUECOLOR:
                    if (bitdepth != 8)
                    {
                        throw new IOException("Unsupported bit depth: " + bitdepth);
                    }
                    bytesPerPixel = 3;
                    break;
                case COLOR_TRUEALPHA:
                    if (bitdepth != 8)
                    {
                        throw new IOException("Unsupported bit depth: " + bitdepth);
                    }
                    bytesPerPixel = 4;
                    break;
                case COLOR_INDEXED:
                    switch (bitdepth)
                    {
                        case 8:
                        case 4:
                        case 2:
                        case 1:
                            bytesPerPixel = 1;
                            break;
                        default:
                            throw new IOException("Unsupported bit depth: " + bitdepth);
                    }
                    break;
                default:
                    throw new IOException("unsupported color format: " + colorType);
            }

            if (buffer[10] != 0)
            {
                throw new IOException("unsupported compression method");
            }
            if (buffer[11] != 0)
            {
                throw new IOException("unsupported filtering method");
            }
            if (buffer[12] != 0)
            {
                throw new IOException("unsupported interlace method");
            }
        }

        private void readPLTE()
        {
            int paletteEntries = chunkLength / 3;
            if (paletteEntries < 1 || paletteEntries > 256 || (chunkLength % 3) != 0)
            {
                throw new IOException("PLTE chunk has wrong length");
            }
            palette = new byte[paletteEntries * 3];
            readChunk(palette, 0, palette.Length);
        }

        private void readtRNS()
        {
            switch (colorType)
            {
                case COLOR_GREYSCALE:
                    checkChunkLength(2);
                    transPixel = new byte[2];
                    readChunk(transPixel, 0, 2);
                    break;
                case COLOR_TRUECOLOR:
                    checkChunkLength(6);
                    transPixel = new byte[6];
                    readChunk(transPixel, 0, 6);
                    break;
                case COLOR_INDEXED:
                    if (palette == null)
                    {
                        throw new IOException("tRNS chunk without PLTE chunk");
                    }
                    paletteA = new byte[palette.Length / 3];

                    for (var i = 0; i < paletteA.Length; i++)
                    {
                        paletteA[i] = 0xFF;
                    }

                    readChunk(paletteA, 0, paletteA.Length);
                    break;
                default:
                    // just ignore it
                    break;
            }
        }

        private void closeChunk()
        {
            if (chunkRemaining > 0)
            {
                // just skip the rest and the CRC
                skip(chunkRemaining + 4);
            }
            else
            {
                readFully(buffer, 0, 4);
                int expectedCrc = readInt(buffer, 0);
                int computedCrc = (int)crc.Value;
                if (computedCrc != expectedCrc)
                {
                    throw new IOException("Invalid CRC");
                }
            }
            chunkRemaining = 0;
            chunkLength = 0;
            chunkType = 0;
        }

        private void openChunk()
        {
            readFully(buffer, 0, 8);
            chunkLength = readInt(buffer, 0);
            chunkType = readInt(buffer, 4);
            chunkRemaining = chunkLength;
            crc.Reset();
            crc.Update(buffer, 4, 4); // only chunkType
        }

        private void openChunk(int expected)
        {
            openChunk();
            if (chunkType != expected)
            {
                throw new IOException("Expected chunk: " + Integer.ToHexString(expected));
            }
        }

        private void checkChunkLength(int expected)
        {
            if (chunkLength != expected)
            {
                throw new IOException("Chunk has wrong size");
            }
        }

        private int readChunk(byte[] buffer, int offset, int length)
        {
            if (length > chunkRemaining)
            {
                length = chunkRemaining;
            }
            readFully(buffer, offset, length);
            crc.Update(buffer, offset, length);
            chunkRemaining -= length;
            return length;
        }

        private void refillInflater(Inflater inflater)
        {
            while (chunkRemaining == 0)
            {
                closeChunk();
                openChunk(IDAT);
            }
            int read = readChunk(buffer, 0, buffer.Length);
            inflater.SetInput(buffer, 0, read);
        }

        private void readChunkUnzip(Inflater inflater, byte[] buffer, int offset, int length)
        {
            try
            {
                do
                {
                    int read = inflater.Inflate(buffer, offset, length);
                    if (read <= 0)
                    {
                        if (inflater.Finished())
                        {
                            throw new EOFException();
                        }
                        if (inflater.NeedsInput())
                        {
                            refillInflater(inflater);
                        }
                        else
                        {
                            throw new IOException("Can't inflate " + length + " bytes");
                        }
                    }
                    else
                    {
                        offset += read;
                        length -= read;
                    }
                } while (length > 0);
            }
            catch (DataFormatException ex)
            {
                throw (IOException)(new IOException("inflate error").InitCause(ex));
            }
        }

        private void readFully(byte[] buffer, int offset, int length)
        {
            do
            {
                int read = input.Read(buffer, offset, length);
                if (read < 0)
                {
                    throw new EOFException();
                }
                offset += read;
                length -= read;
            } while (length > 0);
        }

        private int readInt(byte[] buffer, int offset)
        {
            return ((buffer[offset]) << 24) | ((buffer[offset + 1] & 255) << 16) | ((buffer[offset + 2] & 255) << 8)
                | ((buffer[offset + 3] & 255));
        }

        private void skip(long amount)
        {
            while (amount > 0)
            {
                long skipped = input.Skip(amount);
                if (skipped < 0)
                {
                    throw new EOFException();
                }
                amount -= skipped;
            }
        }

        private static bool checkSignatur(byte[] buffer)
        {
            for (int i = 0; i < SIGNATURE.Length; i++)
            {
                if (buffer[i] != SIGNATURE[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void decodeHeader(InputStream input)
        {
            this.input = input;

            readFully(buffer, 0, SIGNATURE.Length);
            if (!checkSignatur(buffer))
            {
                throw new IOException("Not a valid PNG file");
            }

            openChunk(IHDR);
            readIHDR();
            closeChunk();

        searchIDAT:
            for (; ; )
            {
                openChunk();
                switch (chunkType)
                {
                    case IDAT:
                        goto searchIDAT;
                    case PLTE:
                        readPLTE();
                        break;
                    case tRNS:
                        readtRNS();
                        break;
                }
                closeChunk();
            }

            if (colorType == COLOR_INDEXED && palette == null)
            {
                throw new IOException("Missing PLTE chunk");
            }
        }
    }
}