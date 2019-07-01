/*
 * Copyright 2012 ZXing.Net authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
/*   
'                   _ooOoo_
'                  o8888888o
'                  88" . "88
'                  (| -_- |)
'                  O\  =  /O
'               ____/`---'\____
'             .'  \\|     |//  `.
'            /  \\|||  :  |||//  \
'           /  _||||| -:- |||||-  \
'           |   | \\\  -  /// |   |
'           | \_|  ''\---/''  |   |
'           \  .-\__  `-`  ___/-. /
'         ___`. .'  /--.--\  `. . __
'      ."" '<  `.___\_<|>_/___.'  >'"".
'     | | :  `- \`.;`\ _ /`;.`/ - ` : | |
'     \  \ `-.   \_ __\ /__ _/   .-` /  /
'======`-.____`-.___\_____/___.-`____.-'======
'                   `=---='
'^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
'         佛祖保佑       永无BUG
'========================================================
'作者: MeShen
'日期: 2019.06.21
'========================================================
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace HC.View
{

   public class SharpZXingQRCode
    {
       private static Type renderer = typeof(BitmapRenderer);
        /// <summary>
        /// 获取Zxing的二维码图片
        /// </summary>
        /// <param name="text">二维码文本</param>
        /// <param name="cwidth">宽度</param>
        /// <param name="cheight">高度</param>
        /// <returns></returns>
       public static Image Create(string text,  int cwidth,  int cheight)
        {
            Image result = null;
            if (string.IsNullOrEmpty(text))
            {
                result = null;
            }
            else
            {
                ErrorCorrectionLevel ErrorCorrectionInfo = ErrorCorrectionLevel.M;

                var writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = new QrCodeEncodingOptions
                    {
                        Width = cwidth,
                        Height = cheight,
                        ErrorCorrection = ErrorCorrectionInfo
                    },
                    Renderer = (IBarcodeRenderer<Bitmap>)Activator.CreateInstance(renderer)
                };
                result = writer.Write(text);
            }
            return result;
        }
    }
    public class SharpZXingBarCode
    {
        private static Type renderer = typeof(BitmapRenderer);
        /// <summary>
        /// 获取Zxing的一维码图片
        /// </summary>
        /// <param name="text">一维码文本</param>
        /// <param name="int_barcode_type">一维码格式（1:CODE_39、2:CODE_93、3:CODE_128）</param>
        /// <param name="cwidth">宽度</param>
        /// <param name="cheight">高度</param>
        /// <returns></returns>
        public static Image Create(string text, int int_barcode_type, int cwidth, int cheight)
        {
            Image result = null;
            if (string.IsNullOrEmpty(text))
            {
                result = null;
            }
            else
            {
                BarcodeFormat format = BarcodeFormat.CODE_128;
                switch (int_barcode_type)
                {
                    case 1:
                        format = BarcodeFormat.CODE_39;
                        break;
                    case 2:
                        format = BarcodeFormat.CODE_93;
                        break;
                    case 3:
                        format = BarcodeFormat.CODE_128;
                        break;
                    default:
                        format = BarcodeFormat.CODE_128;
                        break;

                }
                var writer = new BarcodeWriter
                {
                    Format = format,
                    Options = new EncodingOptions
                    {
                        Width = cwidth,
                        Height = cheight
                    },
                    Renderer = (IBarcodeRenderer<Bitmap>)Activator.CreateInstance(renderer)
                };
                result = writer.Write(text);
            }
            return result;
        }
    }
    internal class ErrorLevelConverter : TypeConverter
   {
       public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
       {
           if (sourceType == typeof(ErrorCorrectionLevel))
               return true;
           if (sourceType == typeof(String))
               return true;
           return base.CanConvertFrom(context, sourceType);
       }

       public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
       {
           if (destinationType == typeof(ErrorCorrectionLevel))
               return true;
           return base.CanConvertTo(context, destinationType);
       }

       public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
       {
           var level = value as ErrorCorrectionLevel;
           if (level != null)
           {
               return level.Name;
           }
           if (value is String)
           {
               switch (value.ToString())
               {
                   case "L":
                       return ErrorCorrectionLevel.L;
                   case "M":
                       return ErrorCorrectionLevel.M;
                   case "Q":
                       return ErrorCorrectionLevel.Q;
                   case "H":
                       return ErrorCorrectionLevel.H;
                   default:
                       return null;
               }
           }
           return base.ConvertFrom(context, culture, value);
       }

       public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
       {
           if (value == null)
               return null;
           var level = value as ErrorCorrectionLevel;
           if (level != null)
           {
               return level.Name;
           }
           if (destinationType == typeof(ErrorCorrectionLevel))
           {
               switch (value.ToString())
               {
                   case "L":
                       return ErrorCorrectionLevel.L;
                   case "M":
                       return ErrorCorrectionLevel.M;
                   case "Q":
                       return ErrorCorrectionLevel.Q;
                   case "H":
                       return ErrorCorrectionLevel.H;
                   default:
                       return null;
               }
           }
           return base.ConvertTo(context, culture, value, destinationType);
       }

       public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
       {
           return true;
       }

       public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
       {
           return true;
       }

       public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
       {
           return new StandardValuesCollection(new[] { ErrorCorrectionLevel.L, ErrorCorrectionLevel.M, ErrorCorrectionLevel.Q, ErrorCorrectionLevel.H });
       }
   }
   [Serializable]
   public class QrCodeEncodingOptions : EncodingOptions
   {
       /// <summary>
       /// Specifies what degree of error correction to use, for example in QR Codes.
       /// Type depends on the encoder. For example for QR codes it's type
       /// <see cref="ErrorCorrectionLevel"/>.
       /// </summary>
       [TypeConverter(typeof(ErrorLevelConverter))]
       public ErrorCorrectionLevel ErrorCorrection
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.ERROR_CORRECTION))
               {
                   return (ErrorCorrectionLevel)Hints[EncodeHintType.ERROR_CORRECTION];
               }
               return null;
           }
           set
           {
               if (value == null)
               {
                   if (Hints.ContainsKey(EncodeHintType.ERROR_CORRECTION))
                       Hints.Remove(EncodeHintType.ERROR_CORRECTION);
               }
               else
               {
                   Hints[EncodeHintType.ERROR_CORRECTION] = value;
               }
           }
       }

       /// <summary>
       /// Specifies what character encoding to use where applicable (type <see cref="String"/>)
       /// </summary>
       public string CharacterSet
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.CHARACTER_SET))
               {
                   return (string)Hints[EncodeHintType.CHARACTER_SET];
               }
               return null;
           }
           set
           {
               if (value == null)
               {
                   if (Hints.ContainsKey(EncodeHintType.CHARACTER_SET))
                       Hints.Remove(EncodeHintType.CHARACTER_SET);
               }
               else
               {
                   Hints[EncodeHintType.CHARACTER_SET] = value;
               }
           }
       }

       /// <summary>
       /// Explicitly disables ECI segment when generating QR Code
       /// That is against the specification of QR Code but some
       /// readers have problems if the charset is switched from
       /// ISO-8859-1 (default) to UTF-8 with the necessary ECI segment.
       /// If you set the property to true you can use UTF-8 encoding
       /// and the ECI segment is omitted.
       /// </summary>
       public bool DisableECI
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.DISABLE_ECI))
               {
                   return (bool)Hints[EncodeHintType.DISABLE_ECI];
               }
               return false;
           }
           set
           {
               Hints[EncodeHintType.DISABLE_ECI] = value;
           }
       }

       /// <summary>
       /// Specifies the exact version of QR code to be encoded. An integer, range 1 to 40. If the data specified
       /// cannot fit within the required version, a WriterException will be thrown.
       /// </summary>
       public int? QrVersion
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.QR_VERSION))
               {
                   return (int)Hints[EncodeHintType.QR_VERSION];
               }
               return null;
           }
           set
           {
               if (value == null)
               {
                   if (Hints.ContainsKey(EncodeHintType.QR_VERSION))
                       Hints.Remove(EncodeHintType.QR_VERSION);
               }
               else
               {
                   Hints[EncodeHintType.QR_VERSION] = value.Value;
               }
           }
       }
   }
   public class BarcodeWriter : BarcodeWriter<Bitmap>, IBarcodeWriter
   {
       /// <summary>
       /// Initializes a new instance of the <see cref="BarcodeWriter"/> class.
       /// </summary>
       public BarcodeWriter()
       {
           Renderer = new BitmapRenderer();
       }
   }
   public class BitmapRenderer : IBarcodeRenderer<Bitmap>
   {
       /// <summary>
       /// Gets or sets the foreground color.
       /// </summary>
       /// <value>The foreground color.</value>
       public Color Foreground { get; set; }

       /// <summary>
       /// Gets or sets the background color.
       /// </summary>
       /// <value>The background color.</value>
       public Color Background { get; set; }

#if !WindowsCE
       /// <summary>
       /// Gets or sets the resolution which should be used to create the bitmap
       /// If nothing is set the current system settings are used
       /// </summary>
       public float? DpiX { get; set; }

       /// <summary>
       /// Gets or sets the resolution which should be used to create the bitmap
       /// If nothing is set the current system settings are used
       /// </summary>
       public float? DpiY { get; set; }
#endif

       /// <summary>
       /// Gets or sets the text font.
       /// </summary>
       /// <value>
       /// The text font.
       /// </value>
       public Font TextFont { get; set; }

       private static readonly Font DefaultTextFont;

       static BitmapRenderer()
       {
           try
           {
               DefaultTextFont = new Font("Arial", 10, FontStyle.Regular);
           }
           catch (Exception exc)
           {
               // have to ignore, no better idea
#if !WindowsCE
               System.Diagnostics.Trace.TraceError("default text font (Arial, 10, regular) couldn't be loaded: {0}", exc.Message);
#endif
           }
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="BitmapRenderer"/> class.
       /// </summary>
       public BitmapRenderer()
       {
           Foreground = Color.Black;
           Background = Color.White;
           TextFont = DefaultTextFont;
       }

       /// <summary>
       /// Renders the specified matrix.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <param name="format">The format.</param>
       /// <param name="content">The content.</param>
       /// <returns></returns>
       public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content)
       {
           return Render(matrix, format, content, null);
       }

       /// <summary>
       /// Renders the specified matrix.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <param name="format">The format.</param>
       /// <param name="content">The content.</param>
       /// <param name="options">The options.</param>
       /// <returns></returns>
       virtual public Bitmap Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options)
       {
           var width = matrix.Width;
           var height = matrix.Height;
           var font = TextFont ?? DefaultTextFont;
           var emptyArea = 0;
           var outputContent = font != null &&
                               (options == null || !options.PureBarcode) &&
                               !String.IsNullOrEmpty(content) &&
                               (format == BarcodeFormat.CODE_39 ||
                                format == BarcodeFormat.CODE_93 ||
                                format == BarcodeFormat.CODE_128);

           if (options != null)
           {
               if (options.Width > width)
               {
                   width = options.Width;
               }
               if (options.Height > height)
               {
                   height = options.Height;
               }
           }

           // calculating the scaling factor
           var pixelsizeWidth = width / matrix.Width;
           var pixelsizeHeight = height / matrix.Height;

           if (pixelsizeWidth != pixelsizeHeight)
           {
               if (format == BarcodeFormat.QR_CODE)
               {
                   // symetric scaling
                   pixelsizeHeight = pixelsizeWidth = pixelsizeHeight < pixelsizeWidth ? pixelsizeHeight : pixelsizeWidth;
               }
           }

           // create the bitmap and lock the bits because we need the stride
           // which is the width of the image and possible padding bytes
           var bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
#if !WindowsCE
           var dpiX = DpiX ?? DpiY;
           var dpiY = DpiY ?? DpiX;
           if (dpiX != null)
               bmp.SetResolution(dpiX.Value, dpiY.Value);
#endif
           using (var g = Graphics.FromImage(bmp))
           {
               var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
               try
               {
                   var pixels = new byte[bmpData.Stride * height];
                   var padding = bmpData.Stride - (3 * width);
                   var index = 0;
                   var color = Background;

                   // going through the lines of the matrix
                   for (int y = 0; y < matrix.Height; y++)
                   {
                       // stretching the line by the scaling factor
                       for (var pixelsizeHeightProcessed = 0; pixelsizeHeightProcessed < pixelsizeHeight; pixelsizeHeightProcessed++)
                       {
                           // going through the columns of the current line
                           for (var x = 0; x < matrix.Width; x++)
                           {
                               color = matrix[x, y] ? Foreground : Background;
                               // stretching the columns by the scaling factor
                               for (var pixelsizeWidthProcessed = 0; pixelsizeWidthProcessed < pixelsizeWidth; pixelsizeWidthProcessed++)
                               {
                                   pixels[index++] = color.B;
                                   pixels[index++] = color.G;
                                   pixels[index++] = color.R;
                               }
                           }
                           // fill up to the right if the barcode doesn't fully fit in 
                           for (var x = pixelsizeWidth * matrix.Width; x < width; x++)
                           {
                               pixels[index++] = Background.B;
                               pixels[index++] = Background.G;
                               pixels[index++] = Background.R;
                           }
                           index += padding;
                       }
                   }
                   // fill up to the bottom if the barcode doesn't fully fit in 
                   for (var y = pixelsizeHeight * matrix.Height; y < height; y++)
                   {
                       for (var x = 0; x < width; x++)
                       {
                           pixels[index++] = Background.B;
                           pixels[index++] = Background.G;
                           pixels[index++] = Background.R;
                       }
                       index += padding;
                   }
                   // fill the bottom area with the background color if the content should be written below the barcode
                   if (outputContent)
                   {

                       var textAreaHeight = font.Height;

                       emptyArea = height + 10 > textAreaHeight ? textAreaHeight : 0;

                       if (emptyArea > 0)
                       {
                           index = (width * 3 + padding) * (height - emptyArea);
                           for (int y = height - emptyArea; y < height; y++)
                           {
                               for (var x = 0; x < width; x++)
                               {
                                   pixels[index++] = Background.B;
                                   pixels[index++] = Background.G;
                                   pixels[index++] = Background.R;
                               }
                               index += padding;
                           }
                       }
                   }

                   //Copy the data from the byte array into BitmapData.Scan0
                   Marshal.Copy(pixels, 0, bmpData.Scan0, pixels.Length);
               }
               finally
               {
                   //Unlock the pixels
                   bmp.UnlockBits(bmpData);
               }

               // output content text below the barcode
               if (emptyArea > 0)
               {
                   var brush = new SolidBrush(Foreground);
                   var drawFormat = new StringFormat { Alignment = StringAlignment.Center };
                   g.DrawString(content, font, brush, pixelsizeWidth * matrix.Width / 2, height - emptyArea, drawFormat);
               }
           }

           return bmp;
       }
   }
   public abstract class OneDimensionalCodeWriter : Writer
   {
       /// <summary>
       /// Encode a barcode using the default settings.
       /// </summary>
       /// <param name="contents">The contents to encode in the barcode</param>
       /// <param name="format">The barcode format to generate</param>
       /// <param name="width">The preferred width in pixels</param>
       /// <param name="height">The preferred height in pixels</param>
       /// <returns>
       /// The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)
       /// </returns>
       public BitMatrix encode(String contents, BarcodeFormat format, int width, int height)
       {
           return encode(contents, format, width, height, null);
       }

       /// <summary>
       /// Encode the contents following specified format.
       /// {@code width} and {@code height} are required size. This method may return bigger size
       /// {@code BitMatrix} when specified size is too small. The user can set both {@code width} and
       /// {@code height} to zero to get minimum size barcode. If negative value is set to {@code width}
       /// or {@code height}, {@code IllegalArgumentException} is thrown.
       /// </summary>
       public virtual BitMatrix encode(String contents,
                               BarcodeFormat format,
                               int width,
                               int height,
                               IDictionary<EncodeHintType, object> hints)
       {
           if (String.IsNullOrEmpty(contents))
           {
               throw new ArgumentException("Found empty contents");
           }

           if (width < 0 || height < 0)
           {
               throw new ArgumentException("Negative size is not allowed. Input: "
                                           + width + 'x' + height);
           }

           int sidesMargin = DefaultMargin;
           if (hints != null)
           {
               var sidesMarginInt = hints.ContainsKey(EncodeHintType.MARGIN) ? hints[EncodeHintType.MARGIN] : null;
               if (sidesMarginInt != null)
               {
                   sidesMargin = Convert.ToInt32(sidesMarginInt);
               }
           }

           var code = encode(contents);
           return renderResult(code, width, height, sidesMargin);
       }

       /// <summary>
       /// </summary>
       /// <returns>a byte array of horizontal pixels (0 = white, 1 = black)</returns>
       private static BitMatrix renderResult(bool[] code, int width, int height, int sidesMargin)
       {
           int inputWidth = code.Length;
           // Add quiet zone on both sides.
           int fullWidth = inputWidth + sidesMargin;
           int outputWidth = Math.Max(width, fullWidth);
           int outputHeight = Math.Max(1, height);

           int multiple = outputWidth / fullWidth;
           int leftPadding = (outputWidth - (inputWidth * multiple)) / 2;

           BitMatrix output = new BitMatrix(outputWidth, outputHeight);
           for (int inputX = 0, outputX = leftPadding; inputX < inputWidth; inputX++, outputX += multiple)
           {
               if (code[inputX])
               {
                   output.setRegion(outputX, 0, multiple, outputHeight);
               }
           }
           return output;
       }


       /// <summary>
       /// Appends the given pattern to the target array starting at pos.
       /// </summary>
       /// <param name="target">encode black/white pattern into this array</param>
       /// <param name="pos">position to start encoding at in <c>target</c></param>
       /// <param name="pattern">lengths of black/white runs to encode</param>
       /// <param name="startColor">starting color - false for white, true for black</param>
       /// <returns>the number of elements added to target.</returns>
       protected static int appendPattern(bool[] target, int pos, int[] pattern, bool startColor)
       {
           bool color = startColor;
           int numAdded = 0;
           foreach (int len in pattern)
           {
               for (int j = 0; j < len; j++)
               {
                   target[pos++] = color;
               }
               numAdded += len;
               color = !color; // flip color after each segment
           }
           return numAdded;
       }

       /// <summary>
       /// Gets the default margin.
       /// </summary>
       virtual public int DefaultMargin
       {
           get
           {
               // CodaBar spec requires a side margin to be more than ten times wider than narrow space.
               // This seems like a decent idea for a default for all formats.
               return 10;
           }
       }

       /// <summary>
       /// Encode the contents to bool array expression of one-dimensional barcode.
       /// Start code and end code should be included in result, and side margins should not be included.
       /// </summary>
       /// <param name="contents">barcode contents to encode</param>
       /// <returns>a <c>bool[]</c> of horizontal pixels (false = white, true = black)</returns>
       public abstract bool[] encode(String contents);

       /// <summary>
       /// Calculates the checksum digit modulo10.
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       public static String CalculateChecksumDigitModulo10(String contents)
       {
           var oddsum = 0;
           var evensum = 0;

           for (var index = contents.Length - 1; index >= 0; index -= 2)
           {
               oddsum += (contents[index] - '0');
           }
           for (var index = contents.Length - 2; index >= 0; index -= 2)
           {
               evensum += (contents[index] - '0');
           }

           return contents + ((10 - ((oddsum * 3 + evensum) % 10)) % 10);
       }
   }
   public interface IBarcodeRenderer<out TOutput>
   {
       /// <summary>
       /// Renders the specified matrix to its graphically representation
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <param name="format">The format.</param>
       /// <param name="content">The encoded content of the barcode which should be included in the image.
       /// That can be the numbers below a 1D barcode or something other.</param>
       /// <returns></returns>
       TOutput Render(BitMatrix matrix, BarcodeFormat format, string content);

       /// <summary>
       /// Renders the specified matrix to its graphically representation
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <param name="format">The format.</param>
       /// <param name="content">The encoded content of the barcode which should be included in the image.
       /// That can be the numbers below a 1D barcode or something other.</param>
       /// <param name="options">The options.</param>
       /// <returns></returns>
       TOutput Render(BitMatrix matrix, BarcodeFormat format, string content, EncodingOptions options);
   }
   [Serializable]
   public class EncodingOptions
   {
       /// <summary>
       /// Gets the data container for all options
       /// </summary>
       [Browsable(false)]
       public IDictionary<EncodeHintType, object> Hints { get; set; }

       /// <summary>
       /// Specifies the height of the barcode image
       /// </summary>
       public int Height
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.HEIGHT))
               {
                   return (int)Hints[EncodeHintType.HEIGHT];
               }
               return 0;
           }
           set
           {
               Hints[EncodeHintType.HEIGHT] = value;
           }
       }

       /// <summary>
       /// Specifies the width of the barcode image
       /// </summary>
       public int Width
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.WIDTH))
               {
                   return (int)Hints[EncodeHintType.WIDTH];
               }
               return 0;
           }
           set
           {
               Hints[EncodeHintType.WIDTH] = value;
           }
       }

       /// <summary>
       /// Don't put the content string into the output image.
       /// </summary>
       public bool PureBarcode
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.PURE_BARCODE))
               {
                   return (bool)Hints[EncodeHintType.PURE_BARCODE];
               }
               return false;
           }
           set
           {
               Hints[EncodeHintType.PURE_BARCODE] = value;
           }
       }

       /// <summary>
       /// Specifies margin, in pixels, to use when generating the barcode. The meaning can vary
       /// by format; for example it controls margin before and after the barcode horizontally for
       /// most 1D formats.
       /// </summary>
       public int Margin
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.MARGIN))
               {
                   return (int)Hints[EncodeHintType.MARGIN];
               }
               return 0;
           }
           set
           {
               Hints[EncodeHintType.MARGIN] = value;
           }
       }

       /// <summary>
       /// Specifies whether the data should be encoded to the GS1 standard;
       /// FNC1 character is added in front of the data
       /// </summary>
       public bool GS1Format
       {
           get
           {
               if (Hints.ContainsKey(EncodeHintType.GS1_FORMAT))
               {
                   return (bool)Hints[EncodeHintType.GS1_FORMAT];
               }
               return false;
           }
           set
           {
               Hints[EncodeHintType.GS1_FORMAT] = value;
           }
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="EncodingOptions"/> class.
       /// </summary>
       public EncodingOptions()
       {
           Hints = new Dictionary<EncodeHintType, object>();
       }
   }
   public interface IBarcodeWriterGeneric
   {
       /// <summary>
       /// Get or sets the barcode format which should be generated
       /// (only suitable if MultiFormatWriter is used for property Encoder which is the default)
       /// </summary>
       BarcodeFormat Format { get; set; }

       /// <summary>
       /// Gets or sets the options container for the encoding and renderer process.
       /// </summary>
       EncodingOptions Options { get; set; }

       /// <summary>
       /// Gets or sets the writer which encodes the content to a BitMatrix.
       /// If no value is set the MultiFormatWriter is used.
       /// </summary>
       Writer Encoder { get; set; }

       /// <summary>
       /// Encodes the specified contents.
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       BitMatrix Encode(string contents);
   }
   public class BarcodeWriterGeneric : IBarcodeWriterGeneric
   {
       private EncodingOptions options;

       /// <summary>
       /// Gets or sets the barcode format.
       /// The value is only suitable if the MultiFormatWriter is used.
       /// </summary>
       public BarcodeFormat Format { get; set; }

       /// <summary>
       /// Gets or sets the options container for the encoding and renderer process.
       /// </summary>
       public EncodingOptions Options
       {
           get
           {
               return (options ?? (options = new EncodingOptions { Height = 100, Width = 100 }));
           }
           set
           {
               options = value;
           }
       }

       /// <summary>
       /// Gets or sets the writer which encodes the content to a BitMatrix.
       /// If no value is set the MultiFormatWriter is used.
       /// </summary>
       public Writer Encoder { get; set; }

       /// <summary>
       /// 
       /// </summary>
       public BarcodeWriterGeneric()
       {
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="encoder"></param>
       public BarcodeWriterGeneric(Writer encoder)
       {
           Encoder = encoder;
       }

       /// <summary>
       /// Encodes the specified contents and returns a BitMatrix array.
       /// That array has to be rendered manually or with a IBarcodeRenderer.
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       public BitMatrix Encode(string contents)
       {
           var encoder = Encoder ?? new MultiFormatWriter();
           var currentOptions = Options;
           return encoder.encode(contents, Format, currentOptions.Width, currentOptions.Height, currentOptions.Hints);
       }
   }

   public sealed class MultiFormatWriter : Writer
   {
       private static readonly IDictionary<BarcodeFormat, Func<Writer>> formatMap;

       static MultiFormatWriter()
       {
           formatMap = new Dictionary<BarcodeFormat, Func<Writer>>
                        {
                           {BarcodeFormat.QR_CODE, () => new QRCodeWriter()},
                           {BarcodeFormat.CODE_39, () => new Code39Writer()},
                           {BarcodeFormat.CODE_93, () => new Code93Writer()},
                           {BarcodeFormat.CODE_128, () => new Code128Writer()},
                        };
       }

       /// <summary>
       /// Gets the collection of supported writers.
       /// </summary>
       public static ICollection<BarcodeFormat> SupportedWriters
       {
           get { return formatMap.Keys; }
       }

       /// <summary>
       /// encode the given data
       /// </summary>
       /// <param name="contents"></param>
       /// <param name="format"></param>
       /// <param name="width"></param>
       /// <param name="height"></param>
       /// <returns></returns>
       public BitMatrix encode(String contents, BarcodeFormat format, int width, int height)
       {
           return encode(contents, format, width, height, null);
       }

       /// <summary>
       /// encode the given data
       /// </summary>
       /// <param name="contents"></param>
       /// <param name="format"></param>
       /// <param name="width"></param>
       /// <param name="height"></param>
       /// <param name="hints"></param>
       /// <returns></returns>
       public BitMatrix encode(String contents, BarcodeFormat format, int width, int height, IDictionary<EncodeHintType, object> hints)
       {
           if (!formatMap.ContainsKey(format))
               throw new ArgumentException("No encoder available for format " + format);

           return formatMap[format]().encode(contents, format, width, height, hints);
       }
   }
   public partial interface IBarcodeWriter<out TOutput>
   {
       /// <summary>
       /// Creates a visual representation of the contents
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       TOutput Write(string contents);

       /// <summary>
       /// Returns a rendered instance of the barcode which is given by a BitMatrix.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       TOutput Write(BitMatrix matrix);
   }
   public class BarcodeWriter<TOutput> : BarcodeWriterGeneric, IBarcodeWriter<TOutput>
   {
       /// <summary>
       /// Gets or sets the renderer which should be used to render the encoded BitMatrix.
       /// </summary>
       public IBarcodeRenderer<TOutput> Renderer { get; set; }

       /// <summary>
       /// Encodes the specified contents and returns a rendered instance of the barcode.
       /// For rendering the instance of the property Renderer is used and has to be set before
       /// calling that method.
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       public TOutput Write(string contents)
       {
           if (Renderer == null)
           {
               throw new InvalidOperationException("You have to set a renderer instance.");
           }

           var matrix = Encode(contents);

           return Renderer.Render(matrix, Format, contents, Options);
       }

       /// <summary>
       /// Returns a rendered instance of the barcode which is given by a BitMatrix.
       /// For rendering the instance of the property Renderer is used and has to be set before
       /// calling that method.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       public TOutput Write(BitMatrix matrix)
       {
           if (Renderer == null)
           {
               throw new InvalidOperationException("You have to set a renderer instance.");
           }

           return Renderer.Render(matrix, Format, null, Options);
       }
   }
   public interface IBarcodeWriter
   {
       /// <summary>
       /// Creates a visual representation of the contents
       /// </summary>
       System.Drawing.Bitmap Write(string contents);
       /// <summary>
       /// Returns a rendered instance of the barcode which is given by a BitMatrix.
       /// </summary>
       System.Drawing.Bitmap Write(BitMatrix matrix);

       /// <summary>
       /// Get or sets the barcode format which should be generated
       /// (only suitable if MultiFormatWriter is used for property Encoder which is the default)
       /// </summary>
       BarcodeFormat Format { get; set; }

       /// <summary>
       /// Gets or sets the options container for the encoding and renderer process.
       /// </summary>
       EncodingOptions Options { get; set; }

       /// <summary>
       /// Gets or sets the writer which encodes the content to a BitMatrix.
       /// If no value is set the MultiFormatWriter is used.
       /// </summary>
       Writer Encoder { get; set; }

       /// <summary>
       /// Encodes the specified contents.
       /// </summary>
       /// <param name="contents">The contents.</param>
       /// <returns></returns>
       BitMatrix Encode(string contents);
   }
    public sealed class Code128Writer : OneDimensionalCodeWriter
    {
        internal static int[][] CODE_PATTERNS = {
                                                new[] {2, 1, 2, 2, 2, 2}, // 0
                                                new[] {2, 2, 2, 1, 2, 2},
                                                new[] {2, 2, 2, 2, 2, 1},
                                                new[] {1, 2, 1, 2, 2, 3},
                                                new[] {1, 2, 1, 3, 2, 2},
                                                new[] {1, 3, 1, 2, 2, 2}, // 5
                                                new[] {1, 2, 2, 2, 1, 3},
                                                new[] {1, 2, 2, 3, 1, 2},
                                                new[] {1, 3, 2, 2, 1, 2},
                                                new[] {2, 2, 1, 2, 1, 3},
                                                new[] {2, 2, 1, 3, 1, 2}, // 10
                                                new[] {2, 3, 1, 2, 1, 2},
                                                new[] {1, 1, 2, 2, 3, 2},
                                                new[] {1, 2, 2, 1, 3, 2},
                                                new[] {1, 2, 2, 2, 3, 1},
                                                new[] {1, 1, 3, 2, 2, 2}, // 15
                                                new[] {1, 2, 3, 1, 2, 2},
                                                new[] {1, 2, 3, 2, 2, 1},
                                                new[] {2, 2, 3, 2, 1, 1},
                                                new[] {2, 2, 1, 1, 3, 2},
                                                new[] {2, 2, 1, 2, 3, 1}, // 20
                                                new[] {2, 1, 3, 2, 1, 2},
                                                new[] {2, 2, 3, 1, 1, 2},
                                                new[] {3, 1, 2, 1, 3, 1},
                                                new[] {3, 1, 1, 2, 2, 2},
                                                new[] {3, 2, 1, 1, 2, 2}, // 25
                                                new[] {3, 2, 1, 2, 2, 1},
                                                new[] {3, 1, 2, 2, 1, 2},
                                                new[] {3, 2, 2, 1, 1, 2},
                                                new[] {3, 2, 2, 2, 1, 1},
                                                new[] {2, 1, 2, 1, 2, 3}, // 30
                                                new[] {2, 1, 2, 3, 2, 1},
                                                new[] {2, 3, 2, 1, 2, 1},
                                                new[] {1, 1, 1, 3, 2, 3},
                                                new[] {1, 3, 1, 1, 2, 3},
                                                new[] {1, 3, 1, 3, 2, 1}, // 35
                                                new[] {1, 1, 2, 3, 1, 3},
                                                new[] {1, 3, 2, 1, 1, 3},
                                                new[] {1, 3, 2, 3, 1, 1},
                                                new[] {2, 1, 1, 3, 1, 3},
                                                new[] {2, 3, 1, 1, 1, 3}, // 40
                                                new[] {2, 3, 1, 3, 1, 1},
                                                new[] {1, 1, 2, 1, 3, 3},
                                                new[] {1, 1, 2, 3, 3, 1},
                                                new[] {1, 3, 2, 1, 3, 1},
                                                new[] {1, 1, 3, 1, 2, 3}, // 45
                                                new[] {1, 1, 3, 3, 2, 1},
                                                new[] {1, 3, 3, 1, 2, 1},
                                                new[] {3, 1, 3, 1, 2, 1},
                                                new[] {2, 1, 1, 3, 3, 1},
                                                new[] {2, 3, 1, 1, 3, 1}, // 50
                                                new[] {2, 1, 3, 1, 1, 3},
                                                new[] {2, 1, 3, 3, 1, 1},
                                                new[] {2, 1, 3, 1, 3, 1},
                                                new[] {3, 1, 1, 1, 2, 3},
                                                new[] {3, 1, 1, 3, 2, 1}, // 55
                                                new[] {3, 3, 1, 1, 2, 1},
                                                new[] {3, 1, 2, 1, 1, 3},
                                                new[] {3, 1, 2, 3, 1, 1},
                                                new[] {3, 3, 2, 1, 1, 1},
                                                new[] {3, 1, 4, 1, 1, 1}, // 60
                                                new[] {2, 2, 1, 4, 1, 1},
                                                new[] {4, 3, 1, 1, 1, 1},
                                                new[] {1, 1, 1, 2, 2, 4},
                                                new[] {1, 1, 1, 4, 2, 2},
                                                new[] {1, 2, 1, 1, 2, 4}, // 65
                                                new[] {1, 2, 1, 4, 2, 1},
                                                new[] {1, 4, 1, 1, 2, 2},
                                                new[] {1, 4, 1, 2, 2, 1},
                                                new[] {1, 1, 2, 2, 1, 4},
                                                new[] {1, 1, 2, 4, 1, 2}, // 70
                                                new[] {1, 2, 2, 1, 1, 4},
                                                new[] {1, 2, 2, 4, 1, 1},
                                                new[] {1, 4, 2, 1, 1, 2},
                                                new[] {1, 4, 2, 2, 1, 1},
                                                new[] {2, 4, 1, 2, 1, 1}, // 75
                                                new[] {2, 2, 1, 1, 1, 4},
                                                new[] {4, 1, 3, 1, 1, 1},
                                                new[] {2, 4, 1, 1, 1, 2},
                                                new[] {1, 3, 4, 1, 1, 1},
                                                new[] {1, 1, 1, 2, 4, 2}, // 80
                                                new[] {1, 2, 1, 1, 4, 2},
                                                new[] {1, 2, 1, 2, 4, 1},
                                                new[] {1, 1, 4, 2, 1, 2},
                                                new[] {1, 2, 4, 1, 1, 2},
                                                new[] {1, 2, 4, 2, 1, 1}, // 85
                                                new[] {4, 1, 1, 2, 1, 2},
                                                new[] {4, 2, 1, 1, 1, 2},
                                                new[] {4, 2, 1, 2, 1, 1},
                                                new[] {2, 1, 2, 1, 4, 1},
                                                new[] {2, 1, 4, 1, 2, 1}, // 90
                                                new[] {4, 1, 2, 1, 2, 1},
                                                new[] {1, 1, 1, 1, 4, 3},
                                                new[] {1, 1, 1, 3, 4, 1},
                                                new[] {1, 3, 1, 1, 4, 1},
                                                new[] {1, 1, 4, 1, 1, 3}, // 95
                                                new[] {1, 1, 4, 3, 1, 1},
                                                new[] {4, 1, 1, 1, 1, 3},
                                                new[] {4, 1, 1, 3, 1, 1},
                                                new[] {1, 1, 3, 1, 4, 1},
                                                new[] {1, 1, 4, 1, 3, 1}, // 100
                                                new[] {3, 1, 1, 1, 4, 1},
                                                new[] {4, 1, 1, 1, 3, 1},
                                                new[] {2, 1, 1, 4, 1, 2},
                                                new[] {2, 1, 1, 2, 1, 4},
                                                new[] {2, 1, 1, 2, 3, 2}, // 105
                                                new[] {2, 3, 3, 1, 1, 1, 2}
                                             };

        private const int CODE_START_A = 103;
        private const int CODE_START_B = 104;
        private const int CODE_START_C = 105;
        private const int CODE_CODE_A = 101;
        private const int CODE_CODE_B = 100;
        private const int CODE_CODE_C = 99;
        private const int CODE_STOP = 106;

        // Dummy characters used to specify control characters in input
        private const char ESCAPE_FNC_1 = '\u00f1';
        private const char ESCAPE_FNC_2 = '\u00f2';
        private const char ESCAPE_FNC_3 = '\u00f3';
        private const char ESCAPE_FNC_4 = '\u00f4';

        private const int CODE_FNC_1 = 102;   // Code A, Code B, Code C
        private const int CODE_FNC_2 = 97;    // Code A, Code B
        private const int CODE_FNC_3 = 96;    // Code A, Code B
        private const int CODE_FNC_4_A = 101; // Code A
        private const int CODE_FNC_4_B = 100; // Code B

        // Results of minimal lookahead for code C
        private enum CType
        {
            UNCODABLE,
            ONE_DIGIT,
            TWO_DIGITS,
            FNC_1
        }

        private bool forceCodesetB;

        public override BitMatrix encode(String contents,
                                BarcodeFormat format,
                                int width,
                                int height,
                                IDictionary<EncodeHintType, object> hints)
        {
            if (format != BarcodeFormat.CODE_128)
            {
                throw new ArgumentException("Can only encode CODE_128, but got " + format);
            }

            forceCodesetB = (hints != null &&
                             hints.ContainsKey(EncodeHintType.CODE128_FORCE_CODESET_B) &&
                             hints[EncodeHintType.CODE128_FORCE_CODESET_B] != null &&
                             Convert.ToBoolean(hints[EncodeHintType.CODE128_FORCE_CODESET_B].ToString()));
            ;
            if (hints != null &&
                hints.ContainsKey(EncodeHintType.GS1_FORMAT) &&
                hints[EncodeHintType.GS1_FORMAT] != null &&
                Convert.ToBoolean(hints[EncodeHintType.GS1_FORMAT].ToString()))
            {
                // append the FNC1 character at the first position if not already present
                if (!string.IsNullOrEmpty(contents) && contents[0] != ESCAPE_FNC_1)
                    contents = ESCAPE_FNC_1 + contents;
            }

            return base.encode(contents, format, width, height, hints);
        }

        override public bool[] encode(String contents)
        {
            int length = contents.Length;
            // Check length
            if (length < 1 || length > 80)
            {
                throw new ArgumentException(
                    "Contents length should be between 1 and 80 characters, but got " + length);
            }
            // Check content
            for (int i = 0; i < length; i++)
            {
                char c = contents[i];
                switch (c)
                {
                    case ESCAPE_FNC_1:
                    case ESCAPE_FNC_2:
                    case ESCAPE_FNC_3:
                    case ESCAPE_FNC_4:
                        break;
                    default:
                        if (c > 127)
                            // support for FNC4 isn't implemented, no full Latin-1 character set available at the moment
                            throw new ArgumentException("Bad character in input: " + c);
                        break;
                }
            }

            var patterns = new List<int[]>(); // temporary storage for patterns
            int checkSum = 0;
            int checkWeight = 1;
            int codeSet = 0; // selected code (CODE_CODE_B or CODE_CODE_C)
            int position = 0; // position in contents

            while (position < length)
            {
                //Select code to use
                int newCodeSet = chooseCode(contents, position, codeSet);

                //Get the pattern index
                int patternIndex;
                if (newCodeSet == codeSet)
                {
                    // Encode the current character
                    // First handle escapes
                    switch (contents[position])
                    {
                        case ESCAPE_FNC_1:
                            patternIndex = CODE_FNC_1;
                            break;
                        case ESCAPE_FNC_2:
                            patternIndex = CODE_FNC_2;
                            break;
                        case ESCAPE_FNC_3:
                            patternIndex = CODE_FNC_3;
                            break;
                        case ESCAPE_FNC_4:
                            if (newCodeSet == CODE_CODE_A)
                                patternIndex = CODE_FNC_4_A;
                            else
                                patternIndex = CODE_FNC_4_B;
                            break;
                        default:
                            // Then handle normal characters otherwise
                            switch (codeSet)
                            {
                                case CODE_CODE_A:
                                    patternIndex = contents[position] - ' ';
                                    if (patternIndex < 0)
                                    {
                                        // everything below a space character comes behind the underscore in the code patterns table
                                        patternIndex += '`';
                                    }
                                    break;
                                case CODE_CODE_B:
                                    patternIndex = contents[position] - ' ';
                                    break;
                                default:
                                    // CODE_CODE_C
                                    patternIndex = Int32.Parse(contents.Substring(position, 2));
                                    position++; // Also incremented below
                                    break;
                            }
                            break;
                    }
                    position++;
                }
                else
                {
                    // Should we change the current code?
                    // Do we have a code set?
                    if (codeSet == 0)
                    {
                        // No, we don't have a code set
                        switch (newCodeSet)
                        {
                            case CODE_CODE_A:
                                patternIndex = CODE_START_A;
                                break;
                            case CODE_CODE_B:
                                patternIndex = CODE_START_B;
                                break;
                            default:
                                patternIndex = CODE_START_C;
                                break;
                        }
                    }
                    else
                    {
                        // Yes, we have a code set
                        patternIndex = newCodeSet;
                    }
                    codeSet = newCodeSet;
                }

                // Get the pattern
                patterns.Add(CODE_PATTERNS[patternIndex]);

                // Compute checksum
                checkSum += patternIndex * checkWeight;
                if (position != 0)
                {
                    checkWeight++;
                }
            }

            // Compute and append checksum
            checkSum %= 103;
            patterns.Add(CODE_PATTERNS[checkSum]);

            // Append stop code
            patterns.Add(CODE_PATTERNS[CODE_STOP]);

            // Compute code width
            int codeWidth = 0;
            foreach (int[] pattern in patterns)
            {
                foreach (int width in pattern)
                {
                    codeWidth += width;
                }
            }

            // Compute result
            var result = new bool[codeWidth];
            int pos = 0;
            foreach (int[] pattern in patterns)
            {
                pos += appendPattern(result, pos, pattern, true);
            }

            return result;
        }


        private static CType findCType(String value, int start)
        {
            int last = value.Length;
            if (start >= last)
            {
                return CType.UNCODABLE;
            }
            char c = value[start];
            if (c == ESCAPE_FNC_1)
            {
                return CType.FNC_1;
            }
            if (c < '0' || c > '9')
            {
                return CType.UNCODABLE;
            }
            if (start + 1 >= last)
            {
                return CType.ONE_DIGIT;
            }
            c = value[start + 1];
            if (c < '0' || c > '9')
            {
                return CType.ONE_DIGIT;
            }
            return CType.TWO_DIGITS;
        }

        private int chooseCode(String value, int start, int oldCode)
        {
            CType lookahead = findCType(value, start);
            if (lookahead == CType.ONE_DIGIT)
            {
                return CODE_CODE_B;
            }
            if (lookahead == CType.UNCODABLE)
            {
                if (start < value.Length)
                {
                    var c = value[start];
                    if (c < ' ' || (oldCode == CODE_CODE_A && c < '`'))
                        // can continue in code A, encodes ASCII 0 to 95
                        return CODE_CODE_A;
                }
                return CODE_CODE_B; // no choice
            }
            if (oldCode == CODE_CODE_C)
            {
                // can continue in code C
                return CODE_CODE_C;
            }
            if (oldCode == CODE_CODE_B)
            {
                if (lookahead == CType.FNC_1)
                {
                    return CODE_CODE_B; // can continue in code B
                }
                // Seen two consecutive digits, see what follows
                lookahead = findCType(value, start + 2);
                if (lookahead == CType.UNCODABLE || lookahead == CType.ONE_DIGIT)
                {
                    return CODE_CODE_B; // not worth switching now
                }
                if (lookahead == CType.FNC_1)
                { // two digits, then FNC_1...
                    lookahead = findCType(value, start + 3);
                    if (lookahead == CType.TWO_DIGITS)
                    { // then two more digits, switch
                        return forceCodesetB ? CODE_CODE_B : CODE_CODE_C;
                    }
                    else
                    {
                        return CODE_CODE_B; // otherwise not worth switching
                    }
                }
                // At this point, there are at least 4 consecutive digits.
                // Look ahead to choose whether to switch now or on the next round.
                int index = start + 4;
                while ((lookahead = findCType(value, index)) == CType.TWO_DIGITS)
                {
                    index += 2;
                }
                if (lookahead == CType.ONE_DIGIT)
                { // odd number of digits, switch later
                    return CODE_CODE_B;
                }
                return forceCodesetB ? CODE_CODE_B : CODE_CODE_C; // even number of digits, switch now
            }
            // Here oldCode == 0, which means we are choosing the initial code
            if (lookahead == CType.FNC_1)
            { // ignore FNC_1
                lookahead = findCType(value, start + 1);
            }
            if (lookahead == CType.TWO_DIGITS)
            { // at least two digits, start in code C
                return forceCodesetB ? CODE_CODE_B : CODE_CODE_C;
            }
            return CODE_CODE_B;
        }
    }
    public class Code93Writer : OneDimensionalCodeWriter
    {
        public override BitMatrix encode(String contents, BarcodeFormat format, int width, int height, IDictionary<EncodeHintType, object> hints)
        {
            if (format != BarcodeFormat.CODE_93)
            {
                throw new ArgumentException("Can only encode CODE_93, but got " + format);
            }
            return base.encode(contents, format, width, height, hints);
        }
        // Note that 'abcd' are dummy characters in place of control characters.
        internal const String ALPHABET_STRING = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%abcd*";

        /// <summary>
        /// These represent the encodings of characters, as patterns of wide and narrow bars.
        /// The 9 least-significant bits of each int correspond to the pattern of wide and narrow.
        /// </summary>
        internal static readonly int[] CHARACTER_ENCODINGS = {
                                                    0x114, 0x148, 0x144, 0x142, 0x128, 0x124, 0x122, 0x150, 0x112, 0x10A, // 0-9
                                                    0x1A8, 0x1A4, 0x1A2, 0x194, 0x192, 0x18A, 0x168, 0x164, 0x162, 0x134, // A-J
                                                    0x11A, 0x158, 0x14C, 0x146, 0x12C, 0x116, 0x1B4, 0x1B2, 0x1AC, 0x1A6, // K-T
                                                    0x196, 0x19A, 0x16C, 0x166, 0x136, 0x13A, // U-Z
                                                    0x12E, 0x1D4, 0x1D2, 0x1CA, 0x16E, 0x176, 0x1AE, // - - %
                                                    0x126, 0x1DA, 0x1D6, 0x132, 0x15E, // Control chars? $-*
                                                 };
        private static readonly int ASTERISK_ENCODING = CHARACTER_ENCODINGS[47];
        public override bool[] encode(String contents)
        {
            int length = contents.Length;
            if (length > 80)
            {
                throw new ArgumentException(
                   "Requested contents should be less than 80 digits long, but got " + length);
            }
            //each character is encoded by 9 of 0/1's
            int[] widths = new int[9];

            //length of code + 2 start/stop characters + 2 checksums, each of 9 bits, plus a termination bar
            int codeWidth = (contents.Length + 2 + 2) * 9 + 1;

            //start character (*)
            toIntArray(CHARACTER_ENCODINGS[47], widths);

            bool[] result = new bool[codeWidth];
            int pos = appendPattern(result, 0, widths);

            for (int i = 0; i < length; i++)
            {
                int indexInString = ALPHABET_STRING.IndexOf(contents[i]);
                toIntArray(CHARACTER_ENCODINGS[indexInString], widths);
                pos += appendPattern(result, pos, widths);
            }

            //add two checksums
            int check1 = computeChecksumIndex(contents, 20);
            toIntArray(CHARACTER_ENCODINGS[check1], widths);
            pos += appendPattern(result, pos, widths);

            //append the contents to reflect the first checksum added
            contents += ALPHABET_STRING[check1];

            int check2 = computeChecksumIndex(contents, 15);
            toIntArray(CHARACTER_ENCODINGS[check2], widths);
            pos += appendPattern(result, pos, widths);

            //end character (*)
            toIntArray(CHARACTER_ENCODINGS[47], widths);
            pos += appendPattern(result, pos, widths);

            //termination bar (single black bar)
            result[pos] = true;

            return result;
        }

        private static void toIntArray(int a, int[] toReturn)
        {
            for (int i = 0; i < 9; i++)
            {
                int temp = a & (1 << (8 - i));
                toReturn[i] = temp == 0 ? 0 : 1;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target">output to append to</param>
        /// <param name="pos">start position</param>
        /// <param name="pattern">pattern to append</param>
        /// <param name="startColor">unused</param>
        /// <returns>9</returns>
        [Obsolete("without replacement; intended as an internal-only method")]
        protected new static int appendPattern(bool[] target, int pos, int[] pattern, bool startColor)
        {
            return appendPattern(target, pos, pattern);
        }

        private static int appendPattern(bool[] target, int pos, int[] pattern)
        {
            foreach (var bit in pattern)
            {
                target[pos++] = bit != 0;
            }
            return 9;
        }

        private static int computeChecksumIndex(String contents, int maxWeight)
        {
            int weight = 1;
            int total = 0;

            for (int i = contents.Length - 1; i >= 0; i--)
            {
                int indexInString = ALPHABET_STRING.IndexOf(contents[i]);
                total += indexInString * weight;
                if (++weight > maxWeight)
                {
                    weight = 1;
                }
            }
            return total % 47;
        }
    }
    public sealed class Code39Writer : OneDimensionalCodeWriter
    {
        /// <summary>
        /// Encode the contents following specified format.
        /// {@code width} and {@code height} are required size. This method may return bigger size
        /// {@code BitMatrix} when specified size is too small. The user can set both {@code width} and
        /// {@code height} to zero to get minimum size barcode. If negative value is set to {@code width}
        /// or {@code height}, {@code IllegalArgumentException} is thrown.
        /// </summary>
        /// <param name="contents"></param>
        /// <param name="format"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="hints"></param>
        /// <returns></returns>
        public override BitMatrix encode(String contents,
                                BarcodeFormat format,
                                int width,
                                int height,
                                IDictionary<EncodeHintType, object> hints)
        {
            if (format != BarcodeFormat.CODE_39)
            {
                throw new ArgumentException("Can only encode CODE_39, but got " + format);
            }
            return base.encode(contents, format, width, height, hints);
        }
        internal static String ALPHABET_STRING = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
        /// <summary>
        /// These represent the encodings of characters, as patterns of wide and narrow bars.
        /// The 9 least-significant bits of each int correspond to the pattern of wide and narrow,
        /// with 1s representing "wide" and 0s representing narrow.
        /// </summary>
        internal static int[] CHARACTER_ENCODINGS = {
                                                    0x034, 0x121, 0x061, 0x160, 0x031, 0x130, 0x070, 0x025, 0x124, 0x064, // 0-9
                                                    0x109, 0x049, 0x148, 0x019, 0x118, 0x058, 0x00D, 0x10C, 0x04C, 0x01C, // A-J
                                                    0x103, 0x043, 0x142, 0x013, 0x112, 0x052, 0x007, 0x106, 0x046, 0x016, // K-T
                                                    0x181, 0x0C1, 0x1C0, 0x091, 0x190, 0x0D0, 0x085, 0x184, 0x0C4, 0x0A8, // U-$
                                                    0x0A2, 0x08A, 0x02A // /-%
                                                 };
        internal static readonly int ASTERISK_ENCODING = 0x094;
        /// <summary>
        /// Encode the contents to byte array expression of one-dimensional barcode.
        /// Start code and end code should be included in result, and side margins should not be included.
        /// <returns>a {@code boolean[]} of horizontal pixels (false = white, true = black)</returns>
        /// </summary>
        /// <param name="contents"></param>
        /// <returns></returns>
        public override bool[] encode(String contents)
        {
            int length = contents.Length;
            if (length > 80)
            {
                throw new ArgumentException(
                   "Requested contents should be less than 80 digits long, but got " + length);
            }
            for (int i = 0; i < length; i++)
            {
                int indexInString = ALPHABET_STRING.IndexOf(contents[i]);
                if (indexInString < 0)
                {
                    var unencodable = contents[i];
                    contents = tryToConvertToExtendedMode(contents);
                    if (contents == null)
                        throw new ArgumentException("Requested content contains a non-encodable character: '" + unencodable + "'");
                    length = contents.Length;
                    if (length > 80)
                    {
                        throw new ArgumentException(
                           "Requested contents should be less than 80 digits long, but got " + length + " (extended full ascii mode)");
                    }
                    break;
                }
            }

            int[] widths = new int[9];
            int codeWidth = 24 + 1 + length;
            for (int i = 0; i < length; i++)
            {
                int indexInString =ALPHABET_STRING.IndexOf(contents[i]);
                toIntArray(CHARACTER_ENCODINGS[indexInString], widths);
                foreach (int width in widths)
                {
                    codeWidth += width;
                }
            }
            var result = new bool[codeWidth];
            toIntArray(ASTERISK_ENCODING, widths);
            int pos = appendPattern(result, 0, widths, true);
            int[] narrowWhite = { 1 };
            pos += appendPattern(result, pos, narrowWhite, false);
            //append next character to byte matrix
            for (int i = 0; i < length; i++)
            {
                int indexInString = ALPHABET_STRING.IndexOf(contents[i]);
                toIntArray(CHARACTER_ENCODINGS[indexInString], widths);
                pos += appendPattern(result, pos, widths, true);
                pos += appendPattern(result, pos, narrowWhite, false);
            }
            toIntArray(ASTERISK_ENCODING, widths);
            appendPattern(result, pos, widths, true);
            return result;
        }

        private static void toIntArray(int a, int[] toReturn)
        {
            for (int i = 0; i < 9; i++)
            {
                int temp = a & (1 << (8 - i));
                toReturn[i] = temp == 0 ? 1 : 2;
            }
        }

        private static String tryToConvertToExtendedMode(String contents)
        {
            var length = contents.Length;
            var extendedContent = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                var character = (int)contents[i];
                switch (character)
                {
                    case 0:
                        extendedContent.Append("%U");
                        break;
                    case 32:
                        extendedContent.Append(" ");
                        break;
                    case 45:
                        extendedContent.Append("-");
                        break;
                    case 46:
                        extendedContent.Append(".");
                        break;
                    case 64:
                        extendedContent.Append("%V");
                        break;
                    case 96:
                        extendedContent.Append("%W");
                        break;
                    default:
                        if (character > 0 &&
                            character < 27)
                        {
                            extendedContent.Append("$");
                            extendedContent.Append((char)('A' + (character - 1)));
                        }
                        else if (character > 26 && character < 32)
                        {
                            extendedContent.Append("%");
                            extendedContent.Append((char)('A' + (character - 27)));
                        }
                        else if ((character > ' ' && character < '-') || character == '/' || character == ':')
                        {
                            extendedContent.Append("/");
                            extendedContent.Append((char)('A' + (character - 33)));
                        }
                        else if (character > '/' && character < ':')
                        {
                            extendedContent.Append((char)('0' + (character - 48)));
                        }
                        else if (character > ':' && character < '@')
                        {
                            extendedContent.Append("%");
                            extendedContent.Append((char)('F' + (character - 59)));
                        }
                        else if (character > '@' && character < '[')
                        {
                            extendedContent.Append((char)('A' + (character - 65)));
                        }
                        else if (character > 'Z' && character < '`')
                        {
                            extendedContent.Append("%");
                            extendedContent.Append((char)('K' + (character - 91)));
                        }
                        else if (character > '`' && character < '{')
                        {
                            extendedContent.Append("+");
                            extendedContent.Append((char)('A' + (character - 97)));
                        }
                        else if (character > 'z' && character < 128)
                        {
                            extendedContent.Append("%");
                            extendedContent.Append((char)('P' + (character - 123)));
                        }
                        else
                        {
                            return null;
                        }
                        break;
                }
            }

            return extendedContent.ToString();
        }
    }
    public sealed class QRCodeWriter : Writer
   {
       private const int QUIET_ZONE_SIZE = 4;
       public ByteMatrix encodeToByteMatrix(string contents, BarcodeFormat format, int width, int height, IDictionary<EncodeHintType, object> hints)
       {
           if (contents == null || contents.Length == 0)
           {
               throw new ArgumentException("Found empty contents");
           }
           if (format != BarcodeFormat.QR_CODE)
           {
               throw new ArgumentException("Can only encode QR_CODE, but got " + format);
           }
           if (width < 0 || height < 0)
           {
               throw new ArgumentException(string.Concat(new object[]
                {
            "Requested dimensions are too small: ",
            width,
            'x',
            height
                }));
           }
           ErrorCorrectionLevel ecLevel = ErrorCorrectionLevel.L;
           if (hints != null)
           {
               ErrorCorrectionLevel errorCorrectionLevel = (ErrorCorrectionLevel)hints[EncodeHintType.ERROR_CORRECTION];
               if (errorCorrectionLevel != null)
               {
                   ecLevel = errorCorrectionLevel;
               }
           }
           QRCode qRCode = Encoder.encode(contents, ecLevel);
           return QRCodeWriter.renderResult(qRCode, width, height);
       }
       private static void ConvertBToA(byte[] A_0, byte B_1)
       {
           for (int i = 0; i < A_0.Length; i++)
           {
               A_0[i] = B_1;
           }
       }
       /// <summary>
       /// Convert the BitMatrix to ByteMatrix.
       /// </summary>
       private static ByteMatrix renderResult(QRCode A_0, int A_1, int A_2)
       {
           ByteMatrix matrix = A_0.Matrix;
           int width = matrix.Width;
           int height = matrix.Height;
           int num = width + 8;
           int num2 = height + 8;
           int num3 = Math.Max(A_1, num);
           int num4 = Math.Max(A_2, num2);
           int num5 = Math.Min(num3 / num, num4 / num2);
           int num6 = (num3 - width * num5) / 2;
           int num7 = (num4 - height * num5) / 2;
           ByteMatrix byteMatrix = new ByteMatrix(num3, num4);
           byte[][] array = byteMatrix.Array;
           byte[] array2 = new byte[num3];
           for (int i = 0; i < num7; i++)
           {
               QRCodeWriter.ConvertBToA(array[i], (byte)SupportClass.Identity(255L));
           }
           byte[][] array3 = matrix.Array;
           for (int i = 0; i < height; i++)
           {
               for (int j = 0; j < num6; j++)
               {
                   array2[j] = (byte)SupportClass.Identity(255L);
               }
               int num8 = num6;
               for (int j = 0; j < width; j++)
               {
                   byte b = (byte)((array3[i][j] == 1) ? 0L : SupportClass.Identity(255L));
                   for (int k = 0; k < num5; k++)
                   {
                       array2[num8 + k] = b;
                   }
                   num8 += num5;
               }
               num8 = num6 + width * num5;
               for (int j = num8; j < num3; j++)
               {
                   array2[j] = (byte)SupportClass.Identity(255L);
               }
               num8 = num7 + i * num5;
               for (int k = 0; k < num5; k++)
               {
                   Array.Copy(array2, 0, array[num8 + k], 0, num3);
               }
           }
           int num9 = num7 + height * num5;
           for (int i = num9; i < num4; i++)
           {
               QRCodeWriter.ConvertBToA(array[i], (byte)SupportClass.Identity(255L));
           }
           return byteMatrix;
       }
       /// <summary>
       /// Convert the ByteMatrix to BitMatrix.
       /// </summary>
       /// <param name="matrix">The input matrix.</param>
       /// <param name="reqWidth">The requested width of the image (in pixels) with the Datamatrix code</param>
       /// <param name="reqHeight">The requested height of the image (in pixels) with the Datamatrix code</param>
       /// <returns>The output matrix.</returns>
       private static BitMatrix convertByteMatrixToBitMatrix(ByteMatrix matrix, int reqWidth, int reqHeight)
       {
           var matrixWidth = matrix.Width;
           var matrixHeight = matrix.Height;
           var outputWidth = Math.Max(reqWidth, matrixWidth);
           var outputHeight = Math.Max(reqHeight, matrixHeight);

           int multiple = Math.Min(outputWidth / matrixWidth, outputHeight / matrixHeight);

           int leftPadding = (outputWidth - (matrixWidth * multiple)) / 2;
           int topPadding = (outputHeight - (matrixHeight * multiple)) / 2;

           BitMatrix output;

           // remove padding if requested width and height are too small
           if (reqHeight < matrixHeight || reqWidth < matrixWidth)
           {
               leftPadding = 0;
               topPadding = 0;
               output = new BitMatrix(matrixWidth, matrixHeight);
           }
           else
           {
               output = new BitMatrix(reqWidth, reqHeight);
           }

           output.clear();
           for (int inputY = 0, outputY = topPadding; inputY < matrixHeight; inputY++, outputY += multiple)
           {
               // Write the contents of this row of the bytematrix
               for (int inputX = 0, outputX = leftPadding; inputX < matrixWidth; inputX++, outputX += multiple)
               {
                   if (matrix[inputX, inputY] == 1)
                   {
                       output.setRegion(outputX, outputY, multiple, multiple);
                   }
               }
           }

           return output;
       }
       /// <summary>
       /// Encode a barcode using the default settings.
       /// </summary>
       /// <param name="contents">The contents to encode in the barcode</param>
       /// <param name="format">The barcode format to generate</param>
       /// <param name="width">The preferred width in pixels</param>
       /// <param name="height">The preferred height in pixels</param>
       /// <returns>
       /// The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)
       /// </returns>
       public BitMatrix encode(String contents, BarcodeFormat format, int width, int height)
       {
           return encode(contents, format, width, height, null);
       }

       /// <summary>
       /// </summary>
       /// <param name="contents">The contents to encode in the barcode</param>
       /// <param name="format">The barcode format to generate</param>
       /// <param name="width">The preferred width in pixels</param>
       /// <param name="height">The preferred height in pixels</param>
       /// <param name="hints">Additional parameters to supply to the encoder</param>
       /// <returns>
       /// The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)
       /// </returns>
       public BitMatrix encode(String contents,
                               BarcodeFormat format,
                               int width,
                               int height,
                               IDictionary<EncodeHintType, object> hints)
       {
           if (String.IsNullOrEmpty(contents))
           {
               throw new ArgumentException("Found empty contents");
           }

           if (format != BarcodeFormat.QR_CODE)
           {
               throw new ArgumentException("Can only encode QR_CODE, but got " + format);
           }

           if (width < 0 || height < 0)
           {
               throw new ArgumentException("Requested dimensions are too small: " + width + 'x' + height);
           }

           var errorCorrectionLevel = ErrorCorrectionLevel.L;
           int quietZone = QUIET_ZONE_SIZE;
           if (hints != null)
           {
               if (hints.ContainsKey(EncodeHintType.ERROR_CORRECTION))
               {
                   var requestedECLevel = hints[EncodeHintType.ERROR_CORRECTION];
                   if (requestedECLevel != null)
                   {
                       errorCorrectionLevel = requestedECLevel as ErrorCorrectionLevel;
                       if (errorCorrectionLevel == null)
                       {
                           switch (requestedECLevel.ToString().ToUpper())
                           {
                               case "L":
                                   errorCorrectionLevel = ErrorCorrectionLevel.L;
                                   break;
                               case "M":
                                   errorCorrectionLevel = ErrorCorrectionLevel.M;
                                   break;
                               case "Q":
                                   errorCorrectionLevel = ErrorCorrectionLevel.Q;
                                   break;
                               case "H":
                                   errorCorrectionLevel = ErrorCorrectionLevel.H;
                                   break;
                               default:
                                   errorCorrectionLevel = ErrorCorrectionLevel.L;
                                   break;
                           }
                       }
                   }
               }
               if (hints.ContainsKey(EncodeHintType.MARGIN))
               {
                   var quietZoneInt = hints[EncodeHintType.MARGIN];
                   if (quietZoneInt != null)
                   {
                       quietZone = Convert.ToInt32(quietZoneInt.ToString());
                   }
               }
           }

           var code = Encoder.encode(contents, errorCorrectionLevel, hints);
           return renderResult(code, width, height, quietZone);
       }

       // Note that the input matrix uses 0 == white, 1 == black, while the output matrix uses
       // 0 == black, 255 == white (i.e. an 8 bit greyscale bitmap).
       private static BitMatrix renderResult(QRCode code, int width, int height, int quietZone)
       {
           var input = code.Matrix;
           if (input == null)
           {
               throw new InvalidOperationException();
           }
           int inputWidth = input.Width;
           int inputHeight = input.Height;
           int qrWidth = inputWidth + (quietZone << 1);
           int qrHeight = inputHeight + (quietZone << 1);
           int outputWidth = Math.Max(width, qrWidth);
           int outputHeight = Math.Max(height, qrHeight);

           int multiple = Math.Min(outputWidth / qrWidth, outputHeight / qrHeight);
           // Padding includes both the quiet zone and the extra white pixels to accommodate the requested
           // dimensions. For example, if input is 25x25 the QR will be 33x33 including the quiet zone.
           // If the requested size is 200x160, the multiple will be 4, for a QR of 132x132. These will
           // handle all the padding from 100x100 (the actual QR) up to 200x160.
           int leftPadding = (outputWidth - (inputWidth * multiple)) / 2;
           int topPadding = (outputHeight - (inputHeight * multiple)) / 2;

           var output = new BitMatrix(outputWidth, outputHeight);

           for (int inputY = 0, outputY = topPadding; inputY < inputHeight; inputY++, outputY += multiple)
           {
               // Write the contents of this row of the barcode
               for (int inputX = 0, outputX = leftPadding; inputX < inputWidth; inputX++, outputX += multiple)
               {
                   if (input[inputX, inputY] == 1)
                   {
                       output.setRegion(outputX, outputY, multiple, multiple);
                   }
               }
           }

           return output;
       }
   }
   
    public static class SupportClass
   {
       /*******************************/
       /// <summary>
       /// Copies an array of chars obtained from a String into a specified array of chars
       /// </summary>
       /// <param name="sourceString">The String to get the chars from</param>
       /// <param name="sourceStart">Position of the String to start getting the chars</param>
       /// <param name="sourceEnd">Position of the String to end getting the chars</param>
       /// <param name="destinationArray">Array to return the chars</param>
       /// <param name="destinationStart">Position of the destination array of chars to start storing the chars</param>
       /// <returns>An array of chars</returns>
       public static void GetCharsFromString(System.String sourceString, int sourceStart, int sourceEnd, char[] destinationArray, int destinationStart)
       {
           int sourceCounter = sourceStart;
           int destinationCounter = destinationStart;
           while (sourceCounter < sourceEnd)
           {
               destinationArray[destinationCounter] = (char)sourceString[sourceCounter];
               sourceCounter++;
               destinationCounter++;
           }
       }
       public static long Identity(long literal)
       {
           return literal;
       }
       /*******************************/
       /// <summary>
       /// Sets the capacity for the specified List
       /// </summary>
       /// <param name="vector">The List which capacity will be set</param>
       /// <param name="newCapacity">The new capacity value</param>
       public static void SetCapacity<T>(System.Collections.Generic.IList<T> vector, int newCapacity) where T : new()
       {
           while (newCapacity > vector.Count)
               vector.Add(new T());
           while (newCapacity < vector.Count)
               vector.RemoveAt(vector.Count - 1);
       }

       /// <summary>
       /// Converts a string-Collection to an array
       /// </summary>
       /// <param name="strings">The strings.</param>
       /// <returns></returns>
       public static String[] toStringArray(ICollection<string> strings)
       {
           var result = new String[strings.Count];
           strings.CopyTo(result, 0);
           return result;
       }

       /// <summary>
       /// Joins all elements to one string.
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="separator">The separator.</param>
       /// <param name="values">The values.</param>
       /// <returns></returns>
       public static string Join<T>(string separator, IEnumerable<T> values)
       {
           var builder = new StringBuilder();
           separator = separator ?? String.Empty;
           if (values != null)
           {
               foreach (var value in values)
               {
                   builder.Append(value);
                   builder.Append(separator);
               }
               if (builder.Length > 0)
                   builder.Length -= separator.Length;
           }

           return builder.ToString();
       }

       /// <summary>
       /// Fills the specified array.
       /// (can't use extension method because of .Net 2.0 support)
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="array">The array.</param>
       /// <param name="value">The value.</param>
       public static void Fill<T>(T[] array, T value)
       {
           for (int i = 0; i < array.Length; i++)
           {
               array[i] = value;
           }
       }

       /// <summary>
       /// Fills the specified array.
       /// (can't use extension method because of .Net 2.0 support)
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="array">The array.</param>
       /// <param name="startIndex">The start index.</param>
       /// <param name="endIndex">The end index.</param>
       /// <param name="value">The value.</param>
       public static void Fill<T>(T[] array, int startIndex, int endIndex, T value)
       {
           for (int i = startIndex; i < endIndex; i++)
           {
               array[i] = value;
           }
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="x"></param>
       /// <returns></returns>
       public static string ToBinaryString(int x)
       {
           char[] bits = new char[32];
           int i = 0;

           while (x != 0)
           {
               bits[i++] = (x & 1) == 1 ? '1' : '0';
               x >>= 1;
           }

           Array.Reverse(bits, 0, i);
           return new string(bits);
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="n"></param>
       /// <returns></returns>
       public static int bitCount(int n)
       {
           int ret = 0;
           while (n != 0)
           {
               n &= (n - 1);
               ret++;
           }
           return ret;
       }

       /// <summary>
       /// Savely gets the value of a decoding hint
       /// if hints is null the default is returned
       /// </summary>
       /// <typeparam name="T"></typeparam>
       /// <param name="hints">The hints.</param>
       /// <param name="hintType">Type of the hint.</param>
       /// <param name="default">The @default.</param>
       /// <returns></returns>
       public static T GetValue<T>(IDictionary<DecodeHintType, object> hints, DecodeHintType hintType, T @default)
       {
           // can't use extension method because of .Net 2.0 support

           if (hints == null)
               return @default;
           if (!hints.ContainsKey(hintType))
               return @default;

           return (T)hints[hintType];
       }
   }
   public enum DecodeHintType
   {
       /// <summary>
       /// Unspecified, application-specific hint. Maps to an unspecified <see cref="System.Object" />.
       /// </summary>
       OTHER,

       /// <summary>
       /// Image is a pure monochrome image of a barcode. Doesn't matter what it maps to;
       /// use <see cref="bool" /> = true.
       /// </summary>
       PURE_BARCODE,

       /// <summary>
       /// Image is known to be of one of a few possible formats.
       /// Maps to a <see cref="System.Collections.ICollection" /> of <see cref="BarcodeFormat" />s.
       /// </summary>
       POSSIBLE_FORMATS,

       /// <summary>
       /// Spend more time to try to find a barcode; optimize for accuracy, not speed.
       /// Doesn't matter what it maps to; use <see cref="bool" /> = true.
       /// </summary>
       TRY_HARDER,

       /// <summary>
       /// Specifies what character encoding to use when decoding, where applicable (type String)
       /// </summary>
       CHARACTER_SET,

       /// <summary>
       /// Allowed lengths of encoded data -- reject anything else. Maps to an int[].
       /// </summary>
       ALLOWED_LENGTHS,

       /// <summary>
       /// Assume Code 39 codes employ a check digit. Maps to <see cref="bool" />.
       /// </summary>
       ASSUME_CODE_39_CHECK_DIGIT,

       /// <summary>
       /// The caller needs to be notified via callback when a possible <see cref="ResultPoint" />
       /// is found. Maps to a <see cref="ResultPointCallback" />.
       /// </summary>
       NEED_RESULT_POINT_CALLBACK,

       /// <summary>
       /// Assume MSI codes employ a check digit. Maps to <see cref="bool" />.
       /// </summary>
       ASSUME_MSI_CHECK_DIGIT,

       /// <summary>
       /// if Code39 could be detected try to use extended mode for full ASCII character set
       /// Maps to <see cref="bool" />.
       /// </summary>
       USE_CODE_39_EXTENDED_MODE,

       /// <summary>
       /// Don't fail if a Code39 is detected but can't be decoded in extended mode.
       /// Return the raw Code39 result instead. Maps to <see cref="bool" />.
       /// </summary>
       RELAXED_CODE_39_EXTENDED_MODE,

       /// <summary>
       /// 1D readers supporting rotation with TRY_HARDER enabled.
       /// But BarcodeReader class can do auto-rotating for 1D and 2D codes.
       /// Enabling that option prevents 1D readers doing double rotation.
       /// BarcodeReader enables that option automatically if "global" auto-rotation is enabled.
       /// Maps to <see cref="bool" />.
       /// </summary>
       TRY_HARDER_WITHOUT_ROTATION,

       /// <summary>
       /// Assume the barcode is being processed as a GS1 barcode, and modify behavior as needed.
       /// For example this affects FNC1 handling for Code 128 (aka GS1-128). Doesn't matter what it maps to;
       /// use <see cref="bool" />.
       /// </summary>
       ASSUME_GS1,

       /// <summary>
       /// If true, return the start and end digits in a Codabar barcode instead of stripping them. They
       /// are alpha, whereas the rest are numeric. By default, they are stripped, but this causes them
       /// to not be. Doesn't matter what it maps to; use <see cref="bool" />.
       /// </summary>
       RETURN_CODABAR_START_END,

       /// <summary>
       /// Allowed extension lengths for EAN or UPC barcodes. Other formats will ignore this.
       /// Maps to an int[] of the allowed extension lengths, for example [2], [5], or [2, 5].
       /// If it is optional to have an extension, do not set this hint. If this is set,
       /// and a UPC or EAN barcode is found but an extension is not, then no result will be returned
       /// at all.
       /// </summary>
       ALLOWED_EAN_EXTENSIONS
   }
   public static class MaskUtil
   {
       // Penalty weights from section 6.8.2.1
       private const int N1 = 3;
       private const int N2 = 3;
       private const int N3 = 40;
       private const int N4 = 10;

       /// <summary>
       /// Apply mask penalty rule 1 and return the penalty. Find repetitive cells with the same color and
       /// give penalty to them. Example: 00000 or 11111.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       public static int applyMaskPenaltyRule1(ByteMatrix matrix)
       {
           return applyMaskPenaltyRule1Internal(matrix, true) + applyMaskPenaltyRule1Internal(matrix, false);
       }

       /// <summary>
       /// Apply mask penalty rule 2 and return the penalty. Find 2x2 blocks with the same color and give
       /// penalty to them. This is actually equivalent to the spec's rule, which is to find MxN blocks and give a
       /// penalty proportional to (M-1)x(N-1), because this is the number of 2x2 blocks inside such a block.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       public static int applyMaskPenaltyRule2(ByteMatrix matrix)
       {
           int penalty = 0;
           var array = matrix.Array;
           int width = matrix.Width;
           int height = matrix.Height;
           for (int y = 0; y < height - 1; y++)
           {
               var arrayY = array[y];
               var arrayY1 = array[y + 1];
               for (int x = 0; x < width - 1; x++)
               {
                   int value = arrayY[x];
                   if (value == arrayY[x + 1] && value == arrayY1[x] && value == arrayY1[x + 1])
                   {
                       penalty++;
                   }
               }
           }
           return N2 * penalty;
       }

       /// <summary>
       /// Apply mask penalty rule 3 and return the penalty. Find consecutive cells of 00001011101 or
       /// 10111010000, and give penalty to them.  If we find patterns like 000010111010000, we give
       /// penalties twice (i.e. 40 * 2).
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       public static int applyMaskPenaltyRule3(ByteMatrix matrix)
       {
           int numPenalties = 0;
           byte[][] array = matrix.Array;
           int width = matrix.Width;
           int height = matrix.Height;
           for (int y = 0; y < height; y++)
           {
               for (int x = 0; x < width; x++)
               {
                   byte[] arrayY = array[y];  // We can at least optimize this access
                   if (x + 6 < width &&
                       arrayY[x] == 1 &&
                       arrayY[x + 1] == 0 &&
                       arrayY[x + 2] == 1 &&
                       arrayY[x + 3] == 1 &&
                       arrayY[x + 4] == 1 &&
                       arrayY[x + 5] == 0 &&
                       arrayY[x + 6] == 1 &&
                       (isWhiteHorizontal(arrayY, x - 4, x) || isWhiteHorizontal(arrayY, x + 7, x + 11)))
                   {
                       numPenalties++;
                   }
                   if (y + 6 < height &&
                       array[y][x] == 1 &&
                       array[y + 1][x] == 0 &&
                       array[y + 2][x] == 1 &&
                       array[y + 3][x] == 1 &&
                       array[y + 4][x] == 1 &&
                       array[y + 5][x] == 0 &&
                       array[y + 6][x] == 1 &&
                       (isWhiteVertical(array, x, y - 4, y) || isWhiteVertical(array, x, y + 7, y + 11)))
                   {
                       numPenalties++;
                   }
               }
           }
           return numPenalties * N3;
       }

       private static bool isWhiteHorizontal(byte[] rowArray, int from, int to)
       {
           from = Math.Max(from, 0);
           to = Math.Min(to, rowArray.Length);
           for (int i = from; i < to; i++)
           {
               if (rowArray[i] == 1)
               {
                   return false;
               }
           }
           return true;
       }

       private static bool isWhiteVertical(byte[][] array, int col, int from, int to)
       {
           from = Math.Max(from, 0);
           to = Math.Min(to, array.Length);
           for (int i = from; i < to; i++)
           {
               if (array[i][col] == 1)
               {
                   return false;
               }
           }
           return true;
       }

       /// <summary>
       /// Apply mask penalty rule 4 and return the penalty. Calculate the ratio of dark cells and give
       /// penalty if the ratio is far from 50%. It gives 10 penalty for 5% distance.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <returns></returns>
       public static int applyMaskPenaltyRule4(ByteMatrix matrix)
       {
           int numDarkCells = 0;
           var array = matrix.Array;
           int width = matrix.Width;
           int height = matrix.Height;
           for (int y = 0; y < height; y++)
           {
               var arrayY = array[y];
               for (int x = 0; x < width; x++)
               {
                   if (arrayY[x] == 1)
                   {
                       numDarkCells++;
                   }
               }
           }
           var numTotalCells = matrix.Height * matrix.Width;
           var darkRatio = (double)numDarkCells / numTotalCells;
           var fivePercentVariances = (int)(Math.Abs(darkRatio - 0.5) * 20.0); // * 100.0 / 5.0
           return fivePercentVariances * N4;
       }

       /// <summary>
       /// Return the mask bit for "getMaskPattern" at "x" and "y". See 8.8 of JISX0510:2004 for mask
       /// pattern conditions.
       /// </summary>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <param name="x">The x.</param>
       /// <param name="y">The y.</param>
       /// <returns></returns>
       public static bool getDataMaskBit(int maskPattern, int x, int y)
       {
           int intermediate, temp;
           switch (maskPattern)
           {

               case 0:
                   intermediate = (y + x) & 0x1;
                   break;

               case 1:
                   intermediate = y & 0x1;
                   break;

               case 2:
                   intermediate = x % 3;
                   break;

               case 3:
                   intermediate = (y + x) % 3;
                   break;

               case 4:
                   intermediate = (((int)((uint)y >> 1)) + (x / 3)) & 0x1;
                   break;

               case 5:
                   temp = y * x;
                   intermediate = (temp & 0x1) + (temp % 3);
                   break;

               case 6:
                   temp = y * x;
                   intermediate = (((temp & 0x1) + (temp % 3)) & 0x1);
                   break;

               case 7:
                   temp = y * x;
                   intermediate = (((temp % 3) + ((y + x) & 0x1)) & 0x1);
                   break;

               default:
                   throw new ArgumentException("Invalid mask pattern: " + maskPattern);

           }
           return intermediate == 0;
       }

       /// <summary>
       /// Helper function for applyMaskPenaltyRule1. We need this for doing this calculation in both
       /// vertical and horizontal orders respectively.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       /// <param name="isHorizontal">if set to <c>true</c> [is horizontal].</param>
       /// <returns></returns>
       private static int applyMaskPenaltyRule1Internal(ByteMatrix matrix, bool isHorizontal)
       {
           int penalty = 0;
           int iLimit = isHorizontal ? matrix.Height : matrix.Width;
           int jLimit = isHorizontal ? matrix.Width : matrix.Height;
           var array = matrix.Array;
           for (int i = 0; i < iLimit; i++)
           {
               int numSameBitCells = 0;
               int prevBit = -1;
               for (int j = 0; j < jLimit; j++)
               {
                   int bit = isHorizontal ? array[i][j] : array[j][i];
                   if (bit == prevBit)
                   {
                       numSameBitCells++;
                   }
                   else
                   {
                       if (numSameBitCells >= 5)
                       {
                           penalty += N1 + (numSameBitCells - 5);
                       }
                       numSameBitCells = 1;  // Include the cell itself.
                       prevBit = bit;
                   }
               }
               if (numSameBitCells >= 5)
               {
                   penalty += N1 + (numSameBitCells - 5);
               }
           }
           return penalty;
       }
   }
   public abstract class ECI
   {
       /// <summary>
       /// the ECI value
       /// </summary>
       public virtual int Value { get; set; }

       internal ECI(int val)
       {
           Value = val;
       }

       /// <param name="val">ECI value</param>
       /// <returns><see cref="ECI"/> representing ECI of given value, or null if it is legal but unsupported</returns>
       /// <throws>ArgumentException if ECI value is invalid </throws>
       public static ECI getECIByValue(int val)
       {
           if (val < 0 || val > 999999)
           {
               throw new System.ArgumentException("Bad ECI value: " + val);
           }
           if (val < 900)
           {
               // Character set ECIs use 000000 - 000899
               return CharacterSetECI.getCharacterSetECIByValue(val);
           }
           return null;
       }
   }
   public sealed class CharacterSetECI : ECI
   {
       internal static readonly IDictionary<int, CharacterSetECI> VALUE_TO_ECI;
       internal static readonly IDictionary<string, CharacterSetECI> NAME_TO_ECI;

       private readonly String encodingName;

       public String EncodingName
       {
           get
           {
               return encodingName;
           }

       }

       static CharacterSetECI()
       {
           VALUE_TO_ECI = new Dictionary<int, CharacterSetECI>();
           NAME_TO_ECI = new Dictionary<string, CharacterSetECI>();
           // TODO figure out if these values are even right!
           addCharacterSet(0, "CP437");
           addCharacterSet(1, new[] { "ISO-8859-1", "ISO8859_1" });
           addCharacterSet(2, "CP437");
           addCharacterSet(3, new[] { "ISO-8859-1", "ISO8859_1" });
           addCharacterSet(4, new[] { "ISO-8859-2", "ISO8859_2" });
           addCharacterSet(5, new[] { "ISO-8859-3", "ISO8859_3" });
           addCharacterSet(6, new[] { "ISO-8859-4", "ISO8859_4" });
           addCharacterSet(7, new[] { "ISO-8859-5", "ISO8859_5" });
           addCharacterSet(8, new[] { "ISO-8859-6", "ISO8859_6" });
           addCharacterSet(9, new[] { "ISO-8859-7", "ISO8859_7" });
           addCharacterSet(10, new[] { "ISO-8859-8", "ISO8859_8" });
           addCharacterSet(11, new[] { "ISO-8859-9", "ISO8859_9" });
           addCharacterSet(12, new[] { "ISO-8859-4", "ISO-8859-10", "ISO8859_10" }); // use ISO-8859-4 because ISO-8859-16 isn't supported
           addCharacterSet(13, new[] { "ISO-8859-11", "ISO8859_11" });
           addCharacterSet(15, new[] { "ISO-8859-13", "ISO8859_13" });
           addCharacterSet(16, new[] { "ISO-8859-1", "ISO-8859-14", "ISO8859_14" }); // use ISO-8859-1 because ISO-8859-16 isn't supported
           addCharacterSet(17, new[] { "ISO-8859-15", "ISO8859_15" });
           addCharacterSet(18, new[] { "ISO-8859-3", "ISO-8859-16", "ISO8859_16" }); // use ISO-8859-3 because ISO-8859-16 isn't supported
           addCharacterSet(20, new[] { "SJIS", "SHIFT_JIS", "ISO-2022-JP" });
           addCharacterSet(21, new[] { "WINDOWS-1250", "CP1250" });
           addCharacterSet(22, new[] { "WINDOWS-1251", "CP1251" });
           addCharacterSet(23, new[] { "WINDOWS-1252", "CP1252" });
           addCharacterSet(24, new[] { "WINDOWS-1256", "CP1256" });
           addCharacterSet(25, new[] { "UTF-16BE", "UNICODEBIG" });
           addCharacterSet(26, new[] { "UTF-8", "UTF8" });
           addCharacterSet(27, "US-ASCII");
           addCharacterSet(170, "US-ASCII");
           addCharacterSet(28, "BIG5");
           addCharacterSet(29, new[] { "GB18030", "GB2312", "EUC_CN", "GBK" });
           addCharacterSet(30, new[] { "EUC-KR", "EUC_KR" });
       }

       private CharacterSetECI(int value, String encodingName)
           : base(value)
       {
           this.encodingName = encodingName;
       }

       private static void addCharacterSet(int value, String encodingName)
       {
           var eci = new CharacterSetECI(value, encodingName);
           VALUE_TO_ECI[value] = eci; // can't use valueOf
           NAME_TO_ECI[encodingName] = eci;
       }

       private static void addCharacterSet(int value, String[] encodingNames)
       {
           var eci = new CharacterSetECI(value, encodingNames[0]);
           VALUE_TO_ECI[value] = eci; // can't use valueOf
           foreach (string t in encodingNames)
           {
               NAME_TO_ECI[t] = eci;
           }
       }

       /// <param name="value">character set ECI value</param>
       /// <returns><see cref="CharacterSetECI"/> representing ECI of given value, or null if it is legal but unsupported</returns>
       public static CharacterSetECI getCharacterSetECIByValue(int value)
       {
           if (value < 0 || value >= 900)
           {
               return null;
           }
           return VALUE_TO_ECI[value];
       }

       /// <param name="name">character set ECI encoding name</param>
       /// <returns><see cref="CharacterSetECI"/> representing ECI for character encoding, or null if it is legalbut unsupported</returns>
       public static CharacterSetECI getCharacterSetECIByName(String name)
       {
           return NAME_TO_ECI[name.ToUpper()];
       }
   }
   internal sealed class GenericGFPoly
   {
       private readonly GenericGF field;
       private readonly int[] coefficients;

       /// <summary>
       /// Initializes a new instance of the <see cref="GenericGFPoly"/> class.
       /// </summary>
       /// <param name="field">the {@link GenericGF} instance representing the field to use
       /// to perform computations</param>
       /// <param name="coefficients">coefficients as ints representing elements of GF(size), arranged
       /// from most significant (highest-power term) coefficient to least significant</param>
       /// <exception cref="ArgumentException">if argument is null or empty,
       /// or if leading coefficient is 0 and this is not a
       /// constant polynomial (that is, it is not the monomial "0")</exception>
       internal GenericGFPoly(GenericGF field, int[] coefficients)
       {
           if (coefficients.Length == 0)
           {
               throw new ArgumentException();
           }
           this.field = field;
           int coefficientsLength = coefficients.Length;
           if (coefficientsLength > 1 && coefficients[0] == 0)
           {
               // Leading term must be non-zero for anything except the constant polynomial "0"
               int firstNonZero = 1;
               while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0)
               {
                   firstNonZero++;
               }
               if (firstNonZero == coefficientsLength)
               {
                   this.coefficients = new int[] { 0 };
               }
               else
               {
                   this.coefficients = new int[coefficientsLength - firstNonZero];
                   Array.Copy(coefficients,
                       firstNonZero,
                       this.coefficients,
                       0,
                       this.coefficients.Length);
               }
           }
           else
           {
               this.coefficients = coefficients;
           }
       }

       internal int[] Coefficients
       {
           get { return coefficients; }
       }

       /// <summary>
       /// degree of this polynomial
       /// </summary>
       internal int Degree
       {
           get
           {
               return coefficients.Length - 1;
           }
       }

       /// <summary>
       /// Gets a value indicating whether this <see cref="GenericGFPoly"/> is zero.
       /// </summary>
       /// <value>true iff this polynomial is the monomial "0"</value>
       internal bool isZero
       {
           get { return coefficients[0] == 0; }
       }

       /// <summary>
       /// coefficient of x^degree term in this polynomial
       /// </summary>
       /// <param name="degree">The degree.</param>
       /// <returns>coefficient of x^degree term in this polynomial</returns>
       internal int getCoefficient(int degree)
       {
           return coefficients[coefficients.Length - 1 - degree];
       }

       /// <summary>
       /// evaluation of this polynomial at a given point
       /// </summary>
       /// <param name="a">A.</param>
       /// <returns>evaluation of this polynomial at a given point</returns>
       internal int evaluateAt(int a)
       {
           int result = 0;
           if (a == 0)
           {
               // Just return the x^0 coefficient
               return getCoefficient(0);
           }
           if (a == 1)
           {
               // Just the sum of the coefficients
               foreach (var coefficient in coefficients)
               {
                   result = GenericGF.addOrSubtract(result, coefficient);
               }
               return result;
           }
           result = coefficients[0];
           int size = coefficients.Length;
           for (int i = 1; i < size; i++)
           {
               result = GenericGF.addOrSubtract(field.multiply(a, result), coefficients[i]);
           }
           return result;
       }

       internal GenericGFPoly addOrSubtract(GenericGFPoly other)
       {
           if (!field.Equals(other.field))
           {
               throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
           }
           if (isZero)
           {
               return other;
           }
           if (other.isZero)
           {
               return this;
           }

           int[] smallerCoefficients = this.coefficients;
           int[] largerCoefficients = other.coefficients;
           if (smallerCoefficients.Length > largerCoefficients.Length)
           {
               int[] temp = smallerCoefficients;
               smallerCoefficients = largerCoefficients;
               largerCoefficients = temp;
           }
           int[] sumDiff = new int[largerCoefficients.Length];
           int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
           // Copy high-order terms only found in higher-degree polynomial's coefficients
           Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);

           for (int i = lengthDiff; i < largerCoefficients.Length; i++)
           {
               sumDiff[i] = GenericGF.addOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
           }

           return new GenericGFPoly(field, sumDiff);
       }

       internal GenericGFPoly multiply(GenericGFPoly other)
       {
           if (!field.Equals(other.field))
           {
               throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
           }
           if (isZero || other.isZero)
           {
               return field.Zero;
           }
           int[] aCoefficients = this.coefficients;
           int aLength = aCoefficients.Length;
           int[] bCoefficients = other.coefficients;
           int bLength = bCoefficients.Length;
           int[] product = new int[aLength + bLength - 1];
           for (int i = 0; i < aLength; i++)
           {
               int aCoeff = aCoefficients[i];
               for (int j = 0; j < bLength; j++)
               {
                   product[i + j] = GenericGF.addOrSubtract(product[i + j],
                       field.multiply(aCoeff, bCoefficients[j]));
               }
           }
           return new GenericGFPoly(field, product);
       }

       internal GenericGFPoly multiply(int scalar)
       {
           if (scalar == 0)
           {
               return field.Zero;
           }
           if (scalar == 1)
           {
               return this;
           }
           int size = coefficients.Length;
           int[] product = new int[size];
           for (int i = 0; i < size; i++)
           {
               product[i] = field.multiply(coefficients[i], scalar);
           }
           return new GenericGFPoly(field, product);
       }

       internal GenericGFPoly multiplyByMonomial(int degree, int coefficient)
       {
           if (degree < 0)
           {
               throw new ArgumentException();
           }
           if (coefficient == 0)
           {
               return field.Zero;
           }
           int size = coefficients.Length;
           int[] product = new int[size + degree];
           for (int i = 0; i < size; i++)
           {
               product[i] = field.multiply(coefficients[i], coefficient);
           }
           return new GenericGFPoly(field, product);
       }

       internal GenericGFPoly[] divide(GenericGFPoly other)
       {
           if (!field.Equals(other.field))
           {
               throw new ArgumentException("GenericGFPolys do not have same GenericGF field");
           }
           if (other.isZero)
           {
               throw new ArgumentException("Divide by 0");
           }

           GenericGFPoly quotient = field.Zero;
           GenericGFPoly remainder = this;

           int denominatorLeadingTerm = other.getCoefficient(other.Degree);
           int inverseDenominatorLeadingTerm = field.inverse(denominatorLeadingTerm);

           while (remainder.Degree >= other.Degree && !remainder.isZero)
           {
               int degreeDifference = remainder.Degree - other.Degree;
               int scale = field.multiply(remainder.getCoefficient(remainder.Degree), inverseDenominatorLeadingTerm);
               GenericGFPoly term = other.multiplyByMonomial(degreeDifference, scale);
               GenericGFPoly iterationQuotient = field.buildMonomial(degreeDifference, scale);
               quotient = quotient.addOrSubtract(iterationQuotient);
               remainder = remainder.addOrSubtract(term);
           }

           return new GenericGFPoly[] { quotient, remainder };
       }

       public override String ToString()
       {
           StringBuilder result = new StringBuilder(8 * Degree);
           for (int degree = Degree; degree >= 0; degree--)
           {
               int coefficient = getCoefficient(degree);
               if (coefficient != 0)
               {
                   if (coefficient < 0)
                   {
                       result.Append(" - ");
                       coefficient = -coefficient;
                   }
                   else
                   {
                       if (result.Length > 0)
                       {
                           result.Append(" + ");
                       }
                   }
                   if (degree == 0 || coefficient != 1)
                   {
                       int alphaPower = field.log(coefficient);
                       if (alphaPower == 0)
                       {
                           result.Append('1');
                       }
                       else if (alphaPower == 1)
                       {
                           result.Append('a');
                       }
                       else
                       {
                           result.Append("a^");
                           result.Append(alphaPower);
                       }
                   }
                   if (degree != 0)
                   {
                       if (degree == 1)
                       {
                           result.Append('x');
                       }
                       else
                       {
                           result.Append("x^");
                           result.Append(degree);
                       }
                   }
               }
           }
           return result.ToString();
       }
   }
   public sealed class GenericGF
   {
       public static GenericGF AZTEC_DATA_12 = new GenericGF(0x1069, 4096, 1); // x^12 + x^6 + x^5 + x^3 + 1
       public static GenericGF AZTEC_DATA_10 = new GenericGF(0x409, 1024, 1); // x^10 + x^3 + 1
       public static GenericGF AZTEC_DATA_6 = new GenericGF(0x43, 64, 1); // x^6 + x + 1
       public static GenericGF AZTEC_PARAM = new GenericGF(0x13, 16, 1); // x^4 + x + 1
       public static GenericGF QR_CODE_FIELD_256 = new GenericGF(0x011D, 256, 0); // x^8 + x^4 + x^3 + x^2 + 1
       public static GenericGF DATA_MATRIX_FIELD_256 = new GenericGF(0x012D, 256, 1); // x^8 + x^5 + x^3 + x^2 + 1
       public static GenericGF AZTEC_DATA_8 = DATA_MATRIX_FIELD_256;
       public static GenericGF MAXICODE_FIELD_64 = AZTEC_DATA_6;

       private int[] expTable;
       private int[] logTable;
       private GenericGFPoly zero;
       private GenericGFPoly one;
       private readonly int size;
       private readonly int primitive;
       private readonly int generatorBase;

       /// <summary>
       /// Create a representation of GF(size) using the given primitive polynomial.
       /// </summary>
       /// <param name="primitive">irreducible polynomial whose coefficients are represented by
       /// *  the bits of an int, where the least-significant bit represents the constant
       /// *  coefficient</param>
       /// <param name="size">the size of the field</param>
       /// <param name="genBase">the factor b in the generator polynomial can be 0- or 1-based
       /// *  (g(x) = (x+a^b)(x+a^(b+1))...(x+a^(b+2t-1))).
       /// *  In most cases it should be 1, but for QR code it is 0.</param>
       public GenericGF(int primitive, int size, int genBase)
       {
           this.primitive = primitive;
           this.size = size;
           this.generatorBase = genBase;

           expTable = new int[size];
           logTable = new int[size];
           int x = 1;
           for (int i = 0; i < size; i++)
           {
               expTable[i] = x;
               x <<= 1; // x = x * 2; we're assuming the generator alpha is 2
               if (x >= size)
               {
                   x ^= primitive;
                   x &= size - 1;
               }
           }
           for (int i = 0; i < size - 1; i++)
           {
               logTable[expTable[i]] = i;
           }
           // logTable[0] == 0 but this should never be used
           zero = new GenericGFPoly(this, new int[] { 0 });
           one = new GenericGFPoly(this, new int[] { 1 });
       }

       internal GenericGFPoly Zero
       {
           get
           {
               return zero;
           }
       }

       internal GenericGFPoly One
       {
           get
           {
               return one;
           }
       }

       /// <summary>
       /// Builds the monomial.
       /// </summary>
       /// <param name="degree">The degree.</param>
       /// <param name="coefficient">The coefficient.</param>
       /// <returns>the monomial representing coefficient * x^degree</returns>
       internal GenericGFPoly buildMonomial(int degree, int coefficient)
       {
           if (degree < 0)
           {
               throw new ArgumentException();
           }
           if (coefficient == 0)
           {
               return zero;
           }
           int[] coefficients = new int[degree + 1];
           coefficients[0] = coefficient;
           return new GenericGFPoly(this, coefficients);
       }

       /// <summary>
       /// Implements both addition and subtraction -- they are the same in GF(size).
       /// </summary>
       /// <returns>sum/difference of a and b</returns>
       static internal int addOrSubtract(int a, int b)
       {
           return a ^ b;
       }

       /// <summary>
       /// Exps the specified a.
       /// </summary>
       /// <returns>2 to the power of a in GF(size)</returns>
       internal int exp(int a)
       {
           return expTable[a];
       }

       /// <summary>
       /// Logs the specified a.
       /// </summary>
       /// <param name="a">A.</param>
       /// <returns>base 2 log of a in GF(size)</returns>
       internal int log(int a)
       {
           if (a == 0)
           {
               throw new ArgumentException();
           }
           return logTable[a];
       }

       /// <summary>
       /// Inverses the specified a.
       /// </summary>
       /// <returns>multiplicative inverse of a</returns>
       internal int inverse(int a)
       {
           if (a == 0)
           {
               throw new ArithmeticException();
           }
           return expTable[size - logTable[a] - 1];
       }

       /// <summary>
       /// Multiplies the specified a with b.
       /// </summary>
       /// <param name="a">A.</param>
       /// <param name="b">The b.</param>
       /// <returns>product of a and b in GF(size)</returns>
       internal int multiply(int a, int b)
       {
           if (a == 0 || b == 0)
           {
               return 0;
           }
           return expTable[(logTable[a] + logTable[b]) % (size - 1)];
       }

       /// <summary>
       /// Gets the size.
       /// </summary>
       public int Size
       {
           get { return size; }
       }

       /// <summary>
       /// Gets the generator base.
       /// </summary>
       public int GeneratorBase
       {
           get { return generatorBase; }
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       override public String ToString()
       {
           return "GF(0x" + primitive.ToString("X") + ',' + size + ')';
       }
   }
   public sealed class ReedSolomonEncoder
   {
       private readonly GenericGF field;
       private readonly IList<GenericGFPoly> cachedGenerators;

       public ReedSolomonEncoder(GenericGF field)
       {
           this.field = field;
           this.cachedGenerators = new List<GenericGFPoly>();
           cachedGenerators.Add(new GenericGFPoly(field, new int[] { 1 }));
       }

       private GenericGFPoly buildGenerator(int degree)
       {
           if (degree >= cachedGenerators.Count)
           {
               var lastGenerator = cachedGenerators[cachedGenerators.Count - 1];
               for (int d = cachedGenerators.Count; d <= degree; d++)
               {
                   var nextGenerator = lastGenerator.multiply(new GenericGFPoly(field, new int[] { 1, field.exp(d - 1 + field.GeneratorBase) }));
                   cachedGenerators.Add(nextGenerator);
                   lastGenerator = nextGenerator;
               }
           }
           return cachedGenerators[degree];
       }

       public void encode(int[] toEncode, int ecBytes)
       {
           if (ecBytes == 0)
           {
               throw new ArgumentException("No error correction bytes");
           }
           var dataBytes = toEncode.Length - ecBytes;
           if (dataBytes <= 0)
           {
               throw new ArgumentException("No data bytes provided");
           }

           var generator = buildGenerator(ecBytes);
           var infoCoefficients = new int[dataBytes];
           Array.Copy(toEncode, 0, infoCoefficients, 0, dataBytes);

           var info = new GenericGFPoly(field, infoCoefficients);
           info = info.multiplyByMonomial(ecBytes, 1);

           var remainder = info.divide(generator)[1];
           var coefficients = remainder.Coefficients;
           var numZeroCoefficients = ecBytes - coefficients.Length;
           for (var i = 0; i < numZeroCoefficients; i++)
           {
               toEncode[dataBytes + i] = 0;
           }

           Array.Copy(coefficients, 0, toEncode, dataBytes + numZeroCoefficients, coefficients.Length);
       }
   }
   public static class Encoder
   {

       // The original table is defined in the table 5 of JISX0510:2004 (p.19).
       private static readonly int[] ALPHANUMERIC_TABLE = {
         -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // 0x00-0x0f
         -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,  // 0x10-0x1f
         36, -1, -1, -1, 37, 38, -1, -1, -1, -1, 39, 40, -1, 41, 42, 43,  // 0x20-0x2f
         0,   1,  2,  3,  4,  5,  6,  7,  8,  9, 44, -1, -1, -1, -1, -1,  // 0x30-0x3f
         -1, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,  // 0x40-0x4f
         25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, -1, -1, -1, -1, -1,  // 0x50-0x5f
      };

       internal static String DEFAULT_BYTE_MODE_ENCODING = "ISO-8859-1";

       // The mask penalty calculation is complicated.  See Table 21 of JISX0510:2004 (p.45) for details.
       // Basically it applies four rules and summate all penalties.
       private static int calculateMaskPenalty(ByteMatrix matrix)
       {
           return MaskUtil.applyMaskPenaltyRule1(matrix)
                   + MaskUtil.applyMaskPenaltyRule2(matrix)
                   + MaskUtil.applyMaskPenaltyRule3(matrix)
                   + MaskUtil.applyMaskPenaltyRule4(matrix);
       }

       /// <summary>
       /// Encode "bytes" with the error correction level "ecLevel". The encoding mode will be chosen
       /// internally by chooseMode(). On success, store the result in "qrCode".
       /// We recommend you to use QRCode.EC_LEVEL_L (the lowest level) for
       /// "getECLevel" since our primary use is to show QR code on desktop screens. We don't need very
       /// strong error correction for this purpose.
       /// Note that there is no way to encode bytes in MODE_KANJI. We might want to add EncodeWithMode()
       /// with which clients can specify the encoding mode. For now, we don't need the functionality.
       /// </summary>
       /// <param name="content">text to encode</param>
       /// <param name="ecLevel">error correction level to use</param>
       /// <returns><see cref="QRCode"/> representing the encoded QR code</returns>
       public static QRCode encode(String content, ErrorCorrectionLevel ecLevel)
       {
           return encode(content, ecLevel, null);
       }

       /// <summary>
       /// Encodes the specified content.
       /// </summary>
       /// <param name="content">The content.</param>
       /// <param name="ecLevel">The ec level.</param>
       /// <param name="hints">The hints.</param>
       /// <returns></returns>
       public static QRCode encode(String content,
                                 ErrorCorrectionLevel ecLevel,
                                 IDictionary<EncodeHintType, object> hints)
       {
           // Determine what character encoding has been specified by the caller, if any
           bool hasEncodingHint = hints != null && hints.ContainsKey(EncodeHintType.CHARACTER_SET);


           const string encoding = "UTF-8";
           // caller of the method can only control if the ECI segment should be written
           // character set is fixed to UTF-8; but some scanners doesn't like the ECI segment
           var generateECI = hasEncodingHint;


           // Pick an encoding mode appropriate for the content. Note that this will not attempt to use
           // multiple modes / segments even if that were more efficient. Twould be nice.
           var mode = chooseMode(content, encoding);

           // This will store the header information, like mode and
           // length, as well as "header" segments like an ECI segment.
           var headerBits = new BitArray();

           // Append ECI segment if applicable
           if (mode == Mode.BYTE && generateECI)
           {
               var eci = CharacterSetECI.getCharacterSetECIByName(encoding);
               if (eci != null)
               {
                   var eciIsExplicitDisabled = (hints != null && hints.ContainsKey(EncodeHintType.DISABLE_ECI) && hints[EncodeHintType.DISABLE_ECI] != null && Convert.ToBoolean(hints[EncodeHintType.DISABLE_ECI].ToString()));
                   if (!eciIsExplicitDisabled)
                   {
                       appendECI(eci, headerBits);
                   }
               }
           }

           // Append the FNC1 mode header for GS1 formatted data if applicable
           var hasGS1FormatHint = hints != null && hints.ContainsKey(EncodeHintType.GS1_FORMAT);
           if (hasGS1FormatHint && hints[EncodeHintType.GS1_FORMAT] != null && Convert.ToBoolean(hints[EncodeHintType.GS1_FORMAT].ToString()))
           {
               // GS1 formatted codes are prefixed with a FNC1 in first position mode header
               appendModeInfo(Mode.FNC1_FIRST_POSITION, headerBits);
           }

           // (With ECI in place,) Write the mode marker
           appendModeInfo(mode, headerBits);

           // Collect data within the main segment, separately, to count its size if needed. Don't add it to
           // main payload yet.
           var dataBits = new BitArray();
           appendBytes(content, mode, dataBits, encoding);

           QRVersion version;
           if (hints != null && hints.ContainsKey(EncodeHintType.QR_VERSION))
           {
               int versionNumber = Int32.Parse(hints[EncodeHintType.QR_VERSION].ToString());
               version = QRVersion.getVersionForNumber(versionNumber);
               int bitsNeeded = calculateBitsNeeded(mode, headerBits, dataBits, version);
               if (!willFit(bitsNeeded, version, ecLevel))
               {
                   throw new WriterException("Data too big for requested version");
               }
           }
           else
           {
               version = recommendVersion(ecLevel, mode, headerBits, dataBits);
           }

           var headerAndDataBits = new BitArray();
           headerAndDataBits.appendBitArray(headerBits);
           // Find "length" of main segment and write it
           var numLetters = mode == Mode.BYTE ? dataBits.SizeInBytes : content.Length;
           appendLengthInfo(numLetters, version, mode, headerAndDataBits);
           // Put data together into the overall payload
           headerAndDataBits.appendBitArray(dataBits);

           var ecBlocks = version.getECBlocksForLevel(ecLevel);
           var numDataBytes = version.TotalCodewords - ecBlocks.TotalECCodewords;

           // Terminate the bits properly.
           terminateBits(numDataBytes, headerAndDataBits);

           // Interleave data bits with error correction code.
           var finalBits = interleaveWithECBytes(headerAndDataBits,
                                                      version.TotalCodewords,
                                                      numDataBytes,
                                                      ecBlocks.NumBlocks);

           var qrCode = new QRCode
           {
               ECLevel = ecLevel,
               Mode = mode,
               Version = version
           };

           //  Choose the mask pattern and set to "qrCode".
           var dimension = version.DimensionForVersion;
           var matrix = new ByteMatrix(dimension, dimension);
           var maskPattern = chooseMaskPattern(finalBits, ecLevel, version, matrix);
           qrCode.MaskPattern = maskPattern;

           // Build the matrix and set it to "qrCode".
           MatrixUtil.buildMatrix(finalBits, ecLevel, version, maskPattern, matrix);
           qrCode.Matrix = matrix;

           return qrCode;
       }

       /// <summary>
       /// Decides the smallest version of QR code that will contain all of the provided data.
       /// </summary>
       /// <exception cref="WriterException">if the data cannot fit in any version</exception>
       private static QRVersion recommendVersion(ErrorCorrectionLevel ecLevel, Mode mode, BitArray headerBits, BitArray dataBits)
       {
           // Hard part: need to know version to know how many bits length takes. But need to know how many
           // bits it takes to know version. First we take a guess at version by assuming version will be
           // the minimum, 1:
           var provisionalBitsNeeded = calculateBitsNeeded(mode, headerBits, dataBits, QRVersion.getVersionForNumber(1));
           var provisionalVersion = chooseVersion(provisionalBitsNeeded, ecLevel);

           // Use that guess to calculate the right version. I am still not sure this works in 100% of cases.
           var bitsNeeded = calculateBitsNeeded(mode, headerBits, dataBits, provisionalVersion);
           return chooseVersion(bitsNeeded, ecLevel);
       }

       private static int calculateBitsNeeded(Mode mode, BitArray headerBits, BitArray dataBits, QRVersion version)
       {
           return headerBits.Size + mode.getCharacterCountBits(version) + dataBits.Size;
       }

       /// <summary>
       /// Gets the alphanumeric code.
       /// </summary>
       /// <param name="code">The code.</param>
       /// <returns>the code point of the table used in alphanumeric mode or
       /// -1 if there is no corresponding code in the table.</returns>
       internal static int getAlphanumericCode(int code)
       {
           if (code < ALPHANUMERIC_TABLE.Length)
           {
               return ALPHANUMERIC_TABLE[code];
           }
           return -1;
       }

       /// <summary>
       /// Chooses the mode.
       /// </summary>
       /// <param name="content">The content.</param>
       /// <returns></returns>
       public static Mode chooseMode(String content)
       {
           return chooseMode(content, null);
       }

       /// <summary>
       /// Choose the best mode by examining the content. Note that 'encoding' is used as a hint;
       /// if it is Shift_JIS, and the input is only double-byte Kanji, then we return {@link Mode#KANJI}.
       /// </summary>
       /// <param name="content">The content.</param>
       /// <param name="encoding">The encoding.</param>
       /// <returns></returns>
       private static Mode chooseMode(String content, String encoding)
       {
           if ("Shift_JIS".Equals(encoding) && isOnlyDoubleByteKanji(content))
           {
               // Choose Kanji mode if all input are double-byte characters
               return Mode.KANJI;
           }
           bool hasNumeric = false;
           bool hasAlphanumeric = false;
           for (int i = 0; i < content.Length; ++i)
           {
               char c = content[i];
               if (c >= '0' && c <= '9')
               {
                   hasNumeric = true;
               }
               else if (getAlphanumericCode(c) != -1)
               {
                   hasAlphanumeric = true;
               }
               else
               {
                   return Mode.BYTE;
               }
           }
           if (hasAlphanumeric)
           {

               return Mode.ALPHANUMERIC;
           }
           if (hasNumeric)
           {

               return Mode.NUMERIC;
           }
           return Mode.BYTE;
       }

       private static bool isOnlyDoubleByteKanji(String content)
       {
           byte[] bytes;
           try
           {
               bytes = Encoding.GetEncoding("Shift_JIS").GetBytes(content);
           }
           catch (Exception)
           {
               return false;
           }
           int length = bytes.Length;
           if (length % 2 != 0)
           {
               return false;
           }
           for (int i = 0; i < length; i += 2)
           {


               int byte1 = bytes[i] & 0xFF;
               if ((byte1 < 0x81 || byte1 > 0x9F) && (byte1 < 0xE0 || byte1 > 0xEB))
               {

                   return false;
               }
           }
           return true;
       }

       private static int chooseMaskPattern(BitArray bits,
                                            ErrorCorrectionLevel ecLevel,
                                            QRVersion version,
                                            ByteMatrix matrix)
       {
           int minPenalty = Int32.MaxValue;  // Lower penalty is better.
           int bestMaskPattern = -1;
           // We try all mask patterns to choose the best one.
           for (int maskPattern = 0; maskPattern < QRCode.NUM_MASK_PATTERNS; maskPattern++)
           {

               MatrixUtil.buildMatrix(bits, ecLevel, version, maskPattern, matrix);
               int penalty = calculateMaskPenalty(matrix);
               if (penalty < minPenalty)
               {

                   minPenalty = penalty;
                   bestMaskPattern = maskPattern;
               }
           }
           return bestMaskPattern;
       }

       private static QRVersion chooseVersion(int numInputBits, ErrorCorrectionLevel ecLevel)
       {
           for (int versionNum = 1; versionNum <= 40; versionNum++)
           {
               var version = QRVersion.getVersionForNumber(versionNum);
               if (willFit(numInputBits, version, ecLevel))
               {
                   return version;
               }
           }
           throw new WriterException("Data too big");
       }

       /// <summary></summary>
       /// <returns>true if the number of input bits will fit in a code with the specified version and error correction level.</returns>
       private static bool willFit(int numInputBits, QRVersion version, ErrorCorrectionLevel ecLevel)
       {
           // In the following comments, we use numbers of Version 7-H.
           // numBytes = 196
           var numBytes = version.TotalCodewords;
           // getNumECBytes = 130
           var ecBlocks = version.getECBlocksForLevel(ecLevel);
           var numEcBytes = ecBlocks.TotalECCodewords;
           // getNumDataBytes = 196 - 130 = 66
           var numDataBytes = numBytes - numEcBytes;
           var totalInputBytes = (numInputBits + 7) / 8;
           return numDataBytes >= totalInputBytes;
       }

       /// <summary>
       /// Terminate bits as described in 8.4.8 and 8.4.9 of JISX0510:2004 (p.24).
       /// </summary>
       /// <param name="numDataBytes">The num data bytes.</param>
       /// <param name="bits">The bits.</param>
       internal static void terminateBits(int numDataBytes, BitArray bits)
       {
           int capacity = numDataBytes << 3;
           if (bits.Size > capacity)
           {
               throw new WriterException("data bits cannot fit in the QR Code" + bits.Size + " > " +
                   capacity);
           }
           for (int i = 0; i < 4 && bits.Size < capacity; ++i)
           {
               bits.appendBit(false);
           }
           // Append termination bits. See 8.4.8 of JISX0510:2004 (p.24) for details.
           // If the last byte isn't 8-bit aligned, we'll add padding bits.
           int numBitsInLastByte = bits.Size & 0x07;
           if (numBitsInLastByte > 0)
           {
               for (int i = numBitsInLastByte; i < 8; i++)
               {
                   bits.appendBit(false);
               }
           }
           // If we have more space, we'll fill the space with padding patterns defined in 8.4.9 (p.24).
           int numPaddingBytes = numDataBytes - bits.SizeInBytes;
           for (int i = 0; i < numPaddingBytes; ++i)
           {
               bits.appendBits((i & 0x01) == 0 ? 0xEC : 0x11, 8);
           }
           if (bits.Size != capacity)
           {
               throw new WriterException("Bits size does not equal capacity");
           }
       }

       /// <summary>
       /// Get number of data bytes and number of error correction bytes for block id "blockID". Store
       /// the result in "numDataBytesInBlock", and "numECBytesInBlock". See table 12 in 8.5.1 of
       /// JISX0510:2004 (p.30)
       /// </summary>
       /// <param name="numTotalBytes">The num total bytes.</param>
       /// <param name="numDataBytes">The num data bytes.</param>
       /// <param name="numRSBlocks">The num RS blocks.</param>
       /// <param name="blockID">The block ID.</param>
       /// <param name="numDataBytesInBlock">The num data bytes in block.</param>
       /// <param name="numECBytesInBlock">The num EC bytes in block.</param>
       internal static void getNumDataBytesAndNumECBytesForBlockID(int numTotalBytes,
                                                          int numDataBytes,
                                                          int numRSBlocks,
                                                          int blockID,
                                                          int[] numDataBytesInBlock,
                                                          int[] numECBytesInBlock)
       {
           if (blockID >= numRSBlocks)
           {
               throw new WriterException("Block ID too large");
           }
           // numRsBlocksInGroup2 = 196 % 5 = 1
           int numRsBlocksInGroup2 = numTotalBytes % numRSBlocks;
           // numRsBlocksInGroup1 = 5 - 1 = 4
           int numRsBlocksInGroup1 = numRSBlocks - numRsBlocksInGroup2;
           // numTotalBytesInGroup1 = 196 / 5 = 39
           int numTotalBytesInGroup1 = numTotalBytes / numRSBlocks;
           // numTotalBytesInGroup2 = 39 + 1 = 40
           int numTotalBytesInGroup2 = numTotalBytesInGroup1 + 1;
           // numDataBytesInGroup1 = 66 / 5 = 13
           int numDataBytesInGroup1 = numDataBytes / numRSBlocks;
           // numDataBytesInGroup2 = 13 + 1 = 14
           int numDataBytesInGroup2 = numDataBytesInGroup1 + 1;
           // numEcBytesInGroup1 = 39 - 13 = 26
           int numEcBytesInGroup1 = numTotalBytesInGroup1 - numDataBytesInGroup1;
           // numEcBytesInGroup2 = 40 - 14 = 26
           int numEcBytesInGroup2 = numTotalBytesInGroup2 - numDataBytesInGroup2;
           // Sanity checks.
           // 26 = 26
           if (numEcBytesInGroup1 != numEcBytesInGroup2)
           {

               throw new WriterException("EC bytes mismatch");
           }
           // 5 = 4 + 1.
           if (numRSBlocks != numRsBlocksInGroup1 + numRsBlocksInGroup2)
           {

               throw new WriterException("RS blocks mismatch");
           }
           // 196 = (13 + 26) * 4 + (14 + 26) * 1
           if (numTotalBytes !=
               ((numDataBytesInGroup1 + numEcBytesInGroup1) *
                   numRsBlocksInGroup1) +
                   ((numDataBytesInGroup2 + numEcBytesInGroup2) *
                       numRsBlocksInGroup2))
           {
               throw new WriterException("Total bytes mismatch");
           }

           if (blockID < numRsBlocksInGroup1)
           {

               numDataBytesInBlock[0] = numDataBytesInGroup1;
               numECBytesInBlock[0] = numEcBytesInGroup1;
           }
           else
           {


               numDataBytesInBlock[0] = numDataBytesInGroup2;
               numECBytesInBlock[0] = numEcBytesInGroup2;
           }
       }

       /// <summary>
       /// Interleave "bits" with corresponding error correction bytes. On success, store the result in
       /// "result". The interleave rule is complicated. See 8.6 of JISX0510:2004 (p.37) for details.
       /// </summary>
       /// <param name="bits">The bits.</param>
       /// <param name="numTotalBytes">The num total bytes.</param>
       /// <param name="numDataBytes">The num data bytes.</param>
       /// <param name="numRSBlocks">The num RS blocks.</param>
       /// <returns></returns>
       internal static BitArray interleaveWithECBytes(BitArray bits,
                                              int numTotalBytes,
                                              int numDataBytes,
                                              int numRSBlocks)
       {
           // "bits" must have "getNumDataBytes" bytes of data.
           if (bits.SizeInBytes != numDataBytes)
           {

               throw new WriterException("Number of bits and data bytes does not match");
           }

           // Step 1.  Divide data bytes into blocks and generate error correction bytes for them. We'll
           // store the divided data bytes blocks and error correction bytes blocks into "blocks".
           int dataBytesOffset = 0;
           int maxNumDataBytes = 0;
           int maxNumEcBytes = 0;

           // Since, we know the number of reedsolmon blocks, we can initialize the vector with the number.
           var blocks = new List<BlockPair>(numRSBlocks);

           for (int i = 0; i < numRSBlocks; ++i)
           {

               int[] numDataBytesInBlock = new int[1];
               int[] numEcBytesInBlock = new int[1];
               getNumDataBytesAndNumECBytesForBlockID(
                   numTotalBytes, numDataBytes, numRSBlocks, i,
                   numDataBytesInBlock, numEcBytesInBlock);

               int size = numDataBytesInBlock[0];
               byte[] dataBytes = new byte[size];
               bits.toBytes(8 * dataBytesOffset, dataBytes, 0, size);
               byte[] ecBytes = generateECBytes(dataBytes, numEcBytesInBlock[0]);
               blocks.Add(new BlockPair(dataBytes, ecBytes));

               maxNumDataBytes = Math.Max(maxNumDataBytes, size);
               maxNumEcBytes = Math.Max(maxNumEcBytes, ecBytes.Length);
               dataBytesOffset += numDataBytesInBlock[0];
           }
           if (numDataBytes != dataBytesOffset)
           {

               throw new WriterException("Data bytes does not match offset");
           }

           BitArray result = new BitArray();

           // First, place data blocks.
           for (int i = 0; i < maxNumDataBytes; ++i)
           {
               foreach (BlockPair block in blocks)
               {
                   byte[] dataBytes = block.DataBytes;
                   if (i < dataBytes.Length)
                   {
                       result.appendBits(dataBytes[i], 8);
                   }
               }
           }
           // Then, place error correction blocks.
           for (int i = 0; i < maxNumEcBytes; ++i)
           {
               foreach (BlockPair block in blocks)
               {
                   byte[] ecBytes = block.ErrorCorrectionBytes;
                   if (i < ecBytes.Length)
                   {
                       result.appendBits(ecBytes[i], 8);
                   }
               }
           }
           if (numTotalBytes != result.SizeInBytes)
           {  // Should be same.
               throw new WriterException("Interleaving error: " + numTotalBytes + " and " +
                   result.SizeInBytes + " differ.");
           }

           return result;
       }

       internal static byte[] generateECBytes(byte[] dataBytes, int numEcBytesInBlock)
       {
           int numDataBytes = dataBytes.Length;
           int[] toEncode = new int[numDataBytes + numEcBytesInBlock];
           for (int i = 0; i < numDataBytes; i++)
           {
               toEncode[i] = dataBytes[i] & 0xFF;

           }
           new ReedSolomonEncoder(GenericGF.QR_CODE_FIELD_256).encode(toEncode, numEcBytesInBlock);

           byte[] ecBytes = new byte[numEcBytesInBlock];
           for (int i = 0; i < numEcBytesInBlock; i++)
           {
               ecBytes[i] = (byte)toEncode[numDataBytes + i];

           }
           return ecBytes;
       }

       /// <summary>
       /// Append mode info. On success, store the result in "bits".
       /// </summary>
       /// <param name="mode">The mode.</param>
       /// <param name="bits">The bits.</param>
       internal static void appendModeInfo(Mode mode, BitArray bits)
       {
           bits.appendBits(mode.Bits, 4);
       }


       /// <summary>
       /// Append length info. On success, store the result in "bits".
       /// </summary>
       /// <param name="numLetters">The num letters.</param>
       /// <param name="version">The version.</param>
       /// <param name="mode">The mode.</param>
       /// <param name="bits">The bits.</param>
       internal static void appendLengthInfo(int numLetters, QRVersion version, Mode mode, BitArray bits)
       {
           int numBits = mode.getCharacterCountBits(version);
           if (numLetters >= (1 << numBits))
           {
               throw new WriterException(numLetters + " is bigger than " + ((1 << numBits) - 1));
           }
           bits.appendBits(numLetters, numBits);
       }

       /// <summary>
       /// Append "bytes" in "mode" mode (encoding) into "bits". On success, store the result in "bits".
       /// </summary>
       /// <param name="content">The content.</param>
       /// <param name="mode">The mode.</param>
       /// <param name="bits">The bits.</param>
       /// <param name="encoding">The encoding.</param>
       internal static void appendBytes(String content,
                               Mode mode,
                               BitArray bits,
                               String encoding)
       {
           if (mode.Equals(Mode.NUMERIC))
               appendNumericBytes(content, bits);
           else
               if (mode.Equals(Mode.ALPHANUMERIC))
                   appendAlphanumericBytes(content, bits);
               else
                   if (mode.Equals(Mode.BYTE))
                       append8BitBytes(content, bits, encoding);
                   else
                       if (mode.Equals(Mode.KANJI))
                           appendKanjiBytes(content, bits);
                       else
                           throw new WriterException("Invalid mode: " + mode);
       }

       internal static void appendNumericBytes(String content, BitArray bits)
       {
           int length = content.Length;

           int i = 0;
           while (i < length)
           {
               int num1 = content[i] - '0';
               if (i + 2 < length)
               {
                   // Encode three numeric letters in ten bits.
                   int num2 = content[i + 1] - '0';
                   int num3 = content[i + 2] - '0';
                   bits.appendBits(num1 * 100 + num2 * 10 + num3, 10);
                   i += 3;
               }
               else if (i + 1 < length)
               {
                   // Encode two numeric letters in seven bits.
                   int num2 = content[i + 1] - '0';
                   bits.appendBits(num1 * 10 + num2, 7);
                   i += 2;
               }
               else
               {
                   // Encode one numeric letter in four bits.
                   bits.appendBits(num1, 4);
                   i++;
               }
           }
       }

       internal static void appendAlphanumericBytes(String content, BitArray bits)
       {
           int length = content.Length;

           int i = 0;
           while (i < length)
           {
               int code1 = getAlphanumericCode(content[i]);
               if (code1 == -1)
               {
                   throw new WriterException();
               }
               if (i + 1 < length)
               {
                   int code2 = getAlphanumericCode(content[i + 1]);
                   if (code2 == -1)
                   {
                       throw new WriterException();
                   }
                   // Encode two alphanumeric letters in 11 bits.
                   bits.appendBits(code1 * 45 + code2, 11);
                   i += 2;
               }
               else
               {
                   // Encode one alphanumeric letter in six bits.
                   bits.appendBits(code1, 6);
                   i++;
               }
           }
       }

       internal static void append8BitBytes(String content, BitArray bits, String encoding)
       {
           byte[] bytes;
           try
           {
               bytes = Encoding.GetEncoding(encoding).GetBytes(content);
           }
           catch (Exception uee)
           {
               throw new WriterException(uee.Message, uee);
           }
           foreach (byte b in bytes)
           {
               bits.appendBits(b, 8);
           }
       }

       internal static void appendKanjiBytes(String content, BitArray bits)
       {
           byte[] bytes;
           try
           {
               bytes = Encoding.GetEncoding("Shift_JIS").GetBytes(content);
           }
           catch (Exception uee)
           {
               throw new WriterException(uee.Message, uee);
           }
           int length = bytes.Length;
           for (int i = 0; i < length; i += 2)
           {
               int byte1 = bytes[i] & 0xFF;
               int byte2 = bytes[i + 1] & 0xFF;
               int code = (byte1 << 8) | byte2;
               int subtracted = -1;
               if (code >= 0x8140 && code <= 0x9ffc)
               {

                   subtracted = code - 0x8140;
               }
               else if (code >= 0xe040 && code <= 0xebbf)
               {
                   subtracted = code - 0xc140;
               }
               if (subtracted == -1)
               {

                   throw new WriterException("Invalid byte sequence");
               }
               int encoded = ((subtracted >> 8) * 0xc0) + (subtracted & 0xff);
               bits.appendBits(encoded, 13);
           }
       }

       private static void appendECI(CharacterSetECI eci, BitArray bits)
       {
           bits.appendBits(Mode.ECI.Bits, 4);

           // This is correct for values up to 127, which is all we need now.
           bits.appendBits(eci.Value, 8);
       }
   }
   public static class MatrixUtil
   {
       private static readonly int[][] POSITION_DETECTION_PATTERN = new int[][]
        {
         new int[] {1, 1, 1, 1, 1, 1, 1},
         new int[] {1, 0, 0, 0, 0, 0, 1},
         new int[] {1, 0, 1, 1, 1, 0, 1},
         new int[] {1, 0, 1, 1, 1, 0, 1},
         new int[] {1, 0, 1, 1, 1, 0, 1},
         new int[] {1, 0, 0, 0, 0, 0, 1},
         new int[] {1, 1, 1, 1, 1, 1, 1}
        };

       private static readonly int[][] POSITION_ADJUSTMENT_PATTERN = new int[][]
        {
         new int[] {1, 1, 1, 1, 1},
         new int[] {1, 0, 0, 0, 1},
         new int[] {1, 0, 1, 0, 1},
         new int[] {1, 0, 0, 0, 1},
         new int[] {1, 1, 1, 1, 1}
        };

       // From Appendix E. Table 1, JIS0510X:2004 (p 71). The table was double-checked by komatsu.
       private static readonly int[][] POSITION_ADJUSTMENT_PATTERN_COORDINATE_TABLE = new int[][]
        {
         new int[] {-1, -1, -1, -1, -1, -1, -1},
         new int[] {6, 18, -1, -1, -1, -1, -1},
         new int[] {6, 22, -1, -1, -1, -1, -1},
         new int[] {6, 26, -1, -1, -1, -1, -1},
         new int[] {6, 30, -1, -1, -1, -1, -1},
         new int[] {6, 34, -1, -1, -1, -1, -1},
         new int[] {6, 22, 38, -1, -1, -1, -1},
         new int[] {6, 24, 42, -1, -1, -1, -1},
         new int[] {6, 26, 46, -1, -1, -1, -1},
         new int[] {6, 28, 50, -1, -1, -1, -1},
         new int[] {6, 30, 54, -1, -1, -1, -1},
         new int[] {6, 32, 58, -1, -1, -1, -1},
         new int[] {6, 34, 62, -1, -1, -1, -1},
         new int[] {6, 26, 46, 66, -1, -1, -1},
         new int[] {6, 26, 48, 70, -1, -1, -1},
         new int[] {6, 26, 50, 74, -1, -1, -1},
         new int[] {6, 30, 54, 78, -1, -1, -1},
         new int[] {6, 30, 56, 82, -1, -1, -1},
         new int[] {6, 30, 58, 86, -1, -1, -1},
         new int[] {6, 34, 62, 90, -1, -1, -1},
         new int[] {6, 28, 50, 72, 94, -1, -1},
         new int[] {6, 26, 50, 74, 98, -1, -1},
         new int[] {6, 30, 54, 78, 102, -1, -1},
         new int[] {6, 28, 54, 80, 106, -1, -1},
         new int[] {6, 32, 58, 84, 110, -1, -1},
         new int[] {6, 30, 58, 86, 114, -1, -1},
         new int[] {6, 34, 62, 90, 118, -1, -1},
         new int[] {6, 26, 50, 74, 98, 122, -1},
         new int[] {6, 30, 54, 78, 102, 126, -1},
         new int[] {6, 26, 52, 78, 104, 130, -1},
         new int[] {6, 30, 56, 82, 108, 134, -1},
         new int[] {6, 34, 60, 86, 112, 138, -1},
         new int[] {6, 30, 58, 86, 114, 142, -1},
         new int[] {6, 34, 62, 90, 118, 146, -1},
         new int[] {6, 30, 54, 78, 102, 126, 150},
         new int[] {6, 24, 50, 76, 102, 128, 154},
         new int[] {6, 28, 54, 80, 106, 132, 158},
         new int[] {6, 32, 58, 84, 110, 136, 162},
         new int[] {6, 26, 54, 82, 110, 138, 166},
         new int[] {6, 30, 58, 86, 114, 142, 170}
        };

       // Type info cells at the left top corner.
       private static readonly int[][] TYPE_INFO_COORDINATES = new int[][]
        {
         new int[] {8, 0},
         new int[] {8, 1},
         new int[] {8, 2},
         new int[] {8, 3},
         new int[] {8, 4},
         new int[] {8, 5},
         new int[] {8, 7},
         new int[] {8, 8},
         new int[] {7, 8},
         new int[] {5, 8},
         new int[] {4, 8},
         new int[] {3, 8},
         new int[] {2, 8},
         new int[] {1, 8},
         new int[] {0, 8}
        };

       // From Appendix D in JISX0510:2004 (p. 67)
       private const int VERSION_INFO_POLY = 0x1f25; // 1 1111 0010 0101

       // From Appendix C in JISX0510:2004 (p.65).
       private const int TYPE_INFO_POLY = 0x537;
       private const int TYPE_INFO_MASK_PATTERN = 0x5412;

       /// <summary>
       /// Set all cells to 2.  2 means that the cell is empty (not set yet).
       ///
       /// JAVAPORT: We shouldn't need to do this at all. The code should be rewritten to begin encoding
       /// with the ByteMatrix initialized all to zero.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       public static void clearMatrix(ByteMatrix matrix)
       {
           matrix.clear(2);
       }

       /// <summary>
       /// Build 2D matrix of QR Code from "dataBits" with "ecLevel", "version" and "getMaskPattern". On
       /// success, store the result in "matrix" and return true.
       /// </summary>
       /// <param name="dataBits">The data bits.</param>
       /// <param name="ecLevel">The ec level.</param>
       /// <param name="version">The version.</param>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <param name="matrix">The matrix.</param>
       public static void buildMatrix(BitArray dataBits, ErrorCorrectionLevel ecLevel, QRVersion version, int maskPattern,
          ByteMatrix matrix)
       {
           clearMatrix(matrix);
           embedBasicPatterns(version, matrix);
           // Type information appear with any version.
           embedTypeInfo(ecLevel, maskPattern, matrix);
           // Version info appear if version >= 7.
           maybeEmbedVersionInfo(version, matrix);
           // Data should be embedded at end.
           embedDataBits(dataBits, maskPattern, matrix);
       }

       /// <summary>
       /// Embed basic patterns. On success, modify the matrix and return true.
       /// The basic patterns are:
       /// - Position detection patterns
       /// - Timing patterns
       /// - Dark dot at the left bottom corner
       /// - Position adjustment patterns, if need be
       /// </summary>
       /// <param name="version">The version.</param>
       /// <param name="matrix">The matrix.</param>
       public static void embedBasicPatterns(QRVersion version, ByteMatrix matrix)
       {
           // Let's get started with embedding big squares at corners.
           embedPositionDetectionPatternsAndSeparators(matrix);
           // Then, embed the dark dot at the left bottom corner.
           embedDarkDotAtLeftBottomCorner(matrix);

           // Position adjustment patterns appear if version >= 2.
           maybeEmbedPositionAdjustmentPatterns(version, matrix);
           // Timing patterns should be embedded after position adj. patterns.
           embedTimingPatterns(matrix);
       }

       /// <summary>
       /// Embed type information. On success, modify the matrix.
       /// </summary>
       /// <param name="ecLevel">The ec level.</param>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <param name="matrix">The matrix.</param>
       public static void embedTypeInfo(ErrorCorrectionLevel ecLevel, int maskPattern, ByteMatrix matrix)
       {
           BitArray typeInfoBits = new BitArray();
           makeTypeInfoBits(ecLevel, maskPattern, typeInfoBits);

           for (int i = 0; i < typeInfoBits.Size; ++i)
           {
               // Place bits in LSB to MSB order.  LSB (least significant bit) is the last value in
               // "typeInfoBits".
               int bit = typeInfoBits[typeInfoBits.Size - 1 - i] ? 1 : 0;

               // Type info bits at the left top corner. See 8.9 of JISX0510:2004 (p.46).
               int[] coordinates = TYPE_INFO_COORDINATES[i];
               int x1 = coordinates[0];
               int y1 = coordinates[1];
               matrix[x1, y1] = bit;

               if (i < 8)
               {
                   // Right top corner.
                   int x2 = matrix.Width - i - 1;
                   int y2 = 8;
                   matrix[x2, y2] = bit;
               }
               else
               {
                   // Left bottom corner.
                   int x2 = 8;
                   int y2 = matrix.Height - 7 + (i - 8);
                   matrix[x2, y2] = bit;
               }
           }
       }

       /// <summary>
       /// Embed version information if need be. On success, modify the matrix and return true.
       /// See 8.10 of JISX0510:2004 (p.47) for how to embed version information.
       /// </summary>
       /// <param name="version">The version.</param>
       /// <param name="matrix">The matrix.</param>
       public static void maybeEmbedVersionInfo(QRVersion version, ByteMatrix matrix)
       {
           if (version.VersionNumber < 7)
           {
               // Version info is necessary if version >= 7.
               return; // Don't need version info.
           }
           BitArray versionInfoBits = new BitArray();
           makeVersionInfoBits(version, versionInfoBits);

           int bitIndex = 6 * 3 - 1; // It will decrease from 17 to 0.
           for (int i = 0; i < 6; ++i)
           {
               for (int j = 0; j < 3; ++j)
               {
                   // Place bits in LSB (least significant bit) to MSB order.
                   var bit = versionInfoBits[bitIndex] ? 1 : 0;
                   bitIndex--;
                   // Left bottom corner.
                   matrix[i, matrix.Height - 11 + j] = bit;
                   // Right bottom corner.
                   matrix[matrix.Height - 11 + j, i] = bit;
               }
           }
       }

       /// <summary>
       /// Embed "dataBits" using "getMaskPattern". On success, modify the matrix and return true.
       /// For debugging purposes, it skips masking process if "getMaskPattern" is -1.
       /// See 8.7 of JISX0510:2004 (p.38) for how to embed data bits.
       /// </summary>
       /// <param name="dataBits">The data bits.</param>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <param name="matrix">The matrix.</param>
       public static void embedDataBits(BitArray dataBits, int maskPattern, ByteMatrix matrix)
       {
           int bitIndex = 0;
           int direction = -1;
           // Start from the right bottom cell.
           int x = matrix.Width - 1;
           int y = matrix.Height - 1;
           while (x > 0)
           {
               // Skip the vertical timing pattern.
               if (x == 6)
               {
                   x -= 1;
               }
               while (y >= 0 && y < matrix.Height)
               {
                   for (int i = 0; i < 2; ++i)
                   {
                       int xx = x - i;
                       // Skip the cell if it's not empty.
                       if (!isEmpty(matrix[xx, y]))
                       {
                           continue;
                       }
                       int bit;
                       if (bitIndex < dataBits.Size)
                       {
                           bit = dataBits[bitIndex] ? 1 : 0;
                           ++bitIndex;
                       }
                       else
                       {
                           // Padding bit. If there is no bit left, we'll fill the left cells with 0, as described
                           // in 8.4.9 of JISX0510:2004 (p. 24).
                           bit = 0;
                       }

                       // Skip masking if mask_pattern is -1.
                       if (maskPattern != -1)
                       {
                           if (MaskUtil.getDataMaskBit(maskPattern, xx, y))
                           {
                               bit ^= 0x1;
                           }
                       }
                       matrix[xx, y] = bit;
                   }
                   y += direction;
               }
               direction = -direction; // Reverse the direction.
               y += direction;
               x -= 2; // Move to the left.
           }
           // All bits should be consumed.
           if (bitIndex != dataBits.Size)
           {
               throw new WriterException("Not all bits consumed: " + bitIndex + '/' + dataBits.Size);
           }
       }

       /// <summary>
       /// Return the position of the most significant bit set (to one) in the "value". The most
       /// significant bit is position 32. If there is no bit set, return 0. Examples:
       /// - findMSBSet(0) => 0
       /// - findMSBSet(1) => 1
       /// - findMSBSet(255) => 8
       /// </summary>
       /// <param name="value_Renamed">The value_ renamed.</param>
       /// <returns></returns>
       public static int findMSBSet(int value_Renamed)
       {
           int numDigits = 0;
           while (value_Renamed != 0)
           {
               value_Renamed = (int)((uint)value_Renamed >> 1);
               ++numDigits;
           }
           return numDigits;
       }

       /// <summary>
       /// Calculate BCH (Bose-Chaudhuri-Hocquenghem) code for "value" using polynomial "poly". The BCH
       /// code is used for encoding type information and version information.
       /// Example: Calculation of version information of 7.
       /// f(x) is created from 7.
       ///   - 7 = 000111 in 6 bits
       ///   - f(x) = x^2 + x^2 + x^1
       /// g(x) is given by the standard (p. 67)
       ///   - g(x) = x^12 + x^11 + x^10 + x^9 + x^8 + x^5 + x^2 + 1
       /// Multiply f(x) by x^(18 - 6)
       ///   - f'(x) = f(x) * x^(18 - 6)
       ///   - f'(x) = x^14 + x^13 + x^12
       /// Calculate the remainder of f'(x) / g(x)
       ///         x^2
       ///         __________________________________________________
       ///   g(x) )x^14 + x^13 + x^12
       ///         x^14 + x^13 + x^12 + x^11 + x^10 + x^7 + x^4 + x^2
       ///         --------------------------------------------------
       ///                              x^11 + x^10 + x^7 + x^4 + x^2
       ///
       /// The remainder is x^11 + x^10 + x^7 + x^4 + x^2
       /// Encode it in binary: 110010010100
       /// The return value is 0xc94 (1100 1001 0100)
       ///
       /// Since all coefficients in the polynomials are 1 or 0, we can do the calculation by bit
       /// operations. We don't care if coefficients are positive or negative.
       /// </summary>
       /// <param name="value">The value.</param>
       /// <param name="poly">The poly.</param>
       /// <returns></returns>
       public static int calculateBCHCode(int value, int poly)
       {
           if (poly == 0)
               throw new ArgumentException("0 polynominal", "poly");

           // If poly is "1 1111 0010 0101" (version info poly), msbSetInPoly is 13. We'll subtract 1
           // from 13 to make it 12.
           int msbSetInPoly = findMSBSet(poly);
           value <<= msbSetInPoly - 1;
           // Do the division business using exclusive-or operations.
           while (findMSBSet(value) >= msbSetInPoly)
           {
               value ^= poly << (findMSBSet(value) - msbSetInPoly);
           }
           // Now the "value" is the remainder (i.e. the BCH code)
           return value;
       }

       /// <summary>
       /// Make bit vector of type information. On success, store the result in "bits" and return true.
       /// Encode error correction level and mask pattern. See 8.9 of
       /// JISX0510:2004 (p.45) for details.
       /// </summary>
       /// <param name="ecLevel">The ec level.</param>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <param name="bits">The bits.</param>
       public static void makeTypeInfoBits(ErrorCorrectionLevel ecLevel, int maskPattern, BitArray bits)
       {
           if (!QRCode.isValidMaskPattern(maskPattern))
           {
               throw new WriterException("Invalid mask pattern");
           }
           int typeInfo = (ecLevel.Bits << 3) | maskPattern;
           bits.appendBits(typeInfo, 5);

           int bchCode = calculateBCHCode(typeInfo, TYPE_INFO_POLY);
           bits.appendBits(bchCode, 10);

           BitArray maskBits = new BitArray();
           maskBits.appendBits(TYPE_INFO_MASK_PATTERN, 15);
           bits.xor(maskBits);

           if (bits.Size != 15)
           {
               // Just in case.
               throw new WriterException("should not happen but we got: " + bits.Size);
           }
       }

       /// <summary>
       /// Make bit vector of version information. On success, store the result in "bits" and return true.
       /// See 8.10 of JISX0510:2004 (p.45) for details.
       /// </summary>
       /// <param name="version">The version.</param>
       /// <param name="bits">The bits.</param>
       public static void makeVersionInfoBits(QRVersion version, BitArray bits)
       {
           bits.appendBits(version.VersionNumber, 6);
           int bchCode = calculateBCHCode(version.VersionNumber, VERSION_INFO_POLY);
           bits.appendBits(bchCode, 12);

           if (bits.Size != 18)
           {
               // Just in case.
               throw new WriterException("should not happen but we got: " + bits.Size);
           }
       }

       /// <summary>
       /// Check if "value" is empty.
       /// </summary>
       /// <param name="value">The value.</param>
       /// <returns>
       ///   <c>true</c> if the specified value is empty; otherwise, <c>false</c>.
       /// </returns>
       private static bool isEmpty(int value)
       {
           return value == 2;
       }

       private static void embedTimingPatterns(ByteMatrix matrix)
       {
           // -8 is for skipping position detection patterns (size 7), and two horizontal/vertical
           // separation patterns (size 1). Thus, 8 = 7 + 1.
           for (int i = 8; i < matrix.Width - 8; ++i)
           {
               int bit = (i + 1) % 2;
               // Horizontal line.
               if (isEmpty(matrix[i, 6]))
               {
                   matrix[i, 6] = bit;
               }
               // Vertical line.
               if (isEmpty(matrix[6, i]))
               {
                   matrix[6, i] = bit;
               }
           }
       }

       /// <summary>
       /// Embed the lonely dark dot at left bottom corner. JISX0510:2004 (p.46)
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       private static void embedDarkDotAtLeftBottomCorner(ByteMatrix matrix)
       {
           if (matrix[8, matrix.Height - 8] == 0)
           {
               throw new WriterException();
           }
           matrix[8, matrix.Height - 8] = 1;
       }

       private static void embedHorizontalSeparationPattern(int xStart, int yStart, ByteMatrix matrix)
       {
           for (int x = 0; x < 8; ++x)
           {
               if (!isEmpty(matrix[xStart + x, yStart]))
               {
                   throw new WriterException();
               }
               matrix[xStart + x, yStart] = 0;
           }
       }

       private static void embedVerticalSeparationPattern(int xStart, int yStart, ByteMatrix matrix)
       {
           for (int y = 0; y < 7; ++y)
           {
               if (!isEmpty(matrix[xStart, yStart + y]))
               {
                   throw new WriterException();
               }
               matrix[xStart, yStart + y] = 0;
           }
       }

       /// <summary>
       /// 
       /// </summary>
       /// <param name="xStart">The x start.</param>
       /// <param name="yStart">The y start.</param>
       /// <param name="matrix">The matrix.</param>
       private static void embedPositionAdjustmentPattern(int xStart, int yStart, ByteMatrix matrix)
       {
           for (int y = 0; y < 5; ++y)
           {
               var patternY = POSITION_ADJUSTMENT_PATTERN[y];
               for (int x = 0; x < 5; ++x)
               {
                   matrix[xStart + x, yStart + y] = patternY[x];
               }
           }
       }

       private static void embedPositionDetectionPattern(int xStart, int yStart, ByteMatrix matrix)
       {
           for (int y = 0; y < 7; ++y)
           {
               var patternY = POSITION_DETECTION_PATTERN[y];
               for (int x = 0; x < 7; ++x)
               {
                   matrix[xStart + x, yStart + y] = patternY[x];
               }
           }
       }

       /// <summary>
       /// Embed position detection patterns and surrounding vertical/horizontal separators.
       /// </summary>
       /// <param name="matrix">The matrix.</param>
       private static void embedPositionDetectionPatternsAndSeparators(ByteMatrix matrix)
       {
           // Embed three big squares at corners.
           int pdpWidth = POSITION_DETECTION_PATTERN[0].Length;
           // Left top corner.
           embedPositionDetectionPattern(0, 0, matrix);
           // Right top corner.
           embedPositionDetectionPattern(matrix.Width - pdpWidth, 0, matrix);
           // Left bottom corner.
           embedPositionDetectionPattern(0, matrix.Width - pdpWidth, matrix);

           // Embed horizontal separation patterns around the squares.
           const int hspWidth = 8;
           // Left top corner.
           embedHorizontalSeparationPattern(0, hspWidth - 1, matrix);
           // Right top corner.
           embedHorizontalSeparationPattern(matrix.Width - hspWidth, hspWidth - 1, matrix);
           // Left bottom corner.
           embedHorizontalSeparationPattern(0, matrix.Width - hspWidth, matrix);

           // Embed vertical separation patterns around the squares.
           const int vspSize = 7;
           // Left top corner.
           embedVerticalSeparationPattern(vspSize, 0, matrix);
           // Right top corner.
           embedVerticalSeparationPattern(matrix.Height - vspSize - 1, 0, matrix);
           // Left bottom corner.
           embedVerticalSeparationPattern(vspSize, matrix.Height - vspSize, matrix);
       }

       /// <summary>
       /// Embed position adjustment patterns if need be.
       /// </summary>
       /// <param name="version">The version.</param>
       /// <param name="matrix">The matrix.</param>
       private static void maybeEmbedPositionAdjustmentPatterns(QRVersion version, ByteMatrix matrix)
       {
           if (version.VersionNumber < 2)
           {
               // The patterns appear if version >= 2
               return;
           }
           int index = version.VersionNumber - 1;
           int[] coordinates = POSITION_ADJUSTMENT_PATTERN_COORDINATE_TABLE[index];
           foreach (int y in coordinates)
           {
               if (y >= 0)
               {
                   foreach (int x in coordinates)
                   {
                       if (x >= 0 && isEmpty(matrix[x, y]))
                       {
                           // If the cell is unset, we embed the position adjustment pattern here.
                           // -2 is necessary since the x/y coordinates point to the center of the pattern, not the
                           // left top corner.
                           embedPositionAdjustmentPattern(x - 2, y - 2, matrix);
                       }
                   }
               }
           }
       }
   }
   [Serializable]
   public sealed class WriterException : Exception
   {
       /// <summary>
       /// Initializes a new instance of the <see cref="WriterException"/> class.
       /// </summary>
       public WriterException()
       {
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="WriterException"/> class.
       /// </summary>
       /// <param name="message">The message.</param>
       public WriterException(String message)
           : base(message)
       {
       }

       /// <summary>
       /// Initializes a new instance of the <see cref="WriterException"/> class.
       /// </summary>
       /// <param name="message">The message.</param>
       /// <param name="innerExc">The inner exc.</param>
       public WriterException(String message, Exception innerExc)
           : base(message, innerExc)
       {
       }
   }
   public sealed class ByteMatrix
   {
       private readonly byte[][] bytes;
       private readonly int width;
       private readonly int height;

       /// <summary>
       /// Initializes a new instance of the <see cref="ByteMatrix"/> class.
       /// </summary>
       /// <param name="width">The width.</param>
       /// <param name="height">The height.</param>
       public ByteMatrix(int width, int height)
       {
           bytes = new byte[height][];
           for (var i = 0; i < height; i++)
               bytes[i] = new byte[width];
           this.width = width;
           this.height = height;
       }

       /// <summary>
       /// Gets the height.
       /// </summary>
       public int Height
       {
           get { return height; }
       }

       /// <summary>
       /// Gets the width.
       /// </summary>
       public int Width
       {
           get { return width; }
       }

       /// <summary>
       /// Gets or sets the <see cref="System.Int32"/> with the specified x.
       /// </summary>
       public int this[int x, int y]
       {
           get { return bytes[y][x]; }
           set { bytes[y][x] = (byte)value; }
       }

       /// <summary>
       /// an internal representation as bytes, in row-major order. array[y][x] represents point (x,y)
       /// </summary>
       public byte[][] Array
       {
           get { return bytes; }
       }

       /// <summary>
       /// Sets the specified x.
       /// </summary>
       /// <param name="x">The x.</param>
       /// <param name="y">The y.</param>
       /// <param name="value">The value.</param>
       public void set(int x, int y, byte value)
       {
           bytes[y][x] = value;
       }

       /// <summary>
       /// Sets the specified x.
       /// </summary>
       /// <param name="x">The x.</param>
       /// <param name="y">The y.</param>
       /// <param name="value">if set to <c>true</c> [value].</param>
       public void set(int x, int y, bool value)
       {
           bytes[y][x] = (byte)(value ? 1 : 0);
       }

       /// <summary>
       /// Clears the specified value.
       /// </summary>
       /// <param name="value">The value.</param>
       public void clear(byte value)
       {
           for (int y = 0; y < height; ++y)
           {
               var bytesY = bytes[y];
               for (int x = 0; x < width; ++x)
               {
                   bytesY[x] = value;
               }
           }
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       override public String ToString()
       {
           var result = new StringBuilder(2 * width * height + 2);
           for (int y = 0; y < height; ++y)
           {
               var bytesY = bytes[y];
               for (int x = 0; x < width; ++x)
               {
                   switch (bytesY[x])
                   {
                       case 0:
                           result.Append(" 0");
                           break;
                       case 1:
                           result.Append(" 1");
                           break;
                       default:
                           result.Append("  ");
                           break;
                   }
               }
               result.Append('\n');
           }
           return result.ToString();
       }
   }
   sealed class FormatInformation
   {
       private const int FORMAT_INFO_MASK_QR = 0x5412;

       /// <summary> See ISO 18004:2006, Annex C, Table C.1</summary>
       private static readonly int[][] FORMAT_INFO_DECODE_LOOKUP = new int[][]
                                                                       {
                                                                        new [] { 0x5412, 0x00 },
                                                                        new [] { 0x5125, 0x01 },
                                                                        new [] { 0x5E7C, 0x02 },
                                                                        new [] { 0x5B4B, 0x03 },
                                                                        new [] { 0x45F9, 0x04 },
                                                                        new [] { 0x40CE, 0x05 },
                                                                        new [] { 0x4F97, 0x06 },
                                                                        new [] { 0x4AA0, 0x07 },
                                                                        new [] { 0x77C4, 0x08 },
                                                                        new [] { 0x72F3, 0x09 },
                                                                        new [] { 0x7DAA, 0x0A },
                                                                        new [] { 0x789D, 0x0B },
                                                                        new [] { 0x662F, 0x0C },
                                                                        new [] { 0x6318, 0x0D },
                                                                        new [] { 0x6C41, 0x0E },
                                                                        new [] { 0x6976, 0x0F },
                                                                        new [] { 0x1689, 0x10 },
                                                                        new [] { 0x13BE, 0x11 },
                                                                        new [] { 0x1CE7, 0x12 },
                                                                        new [] { 0x19D0, 0x13 },
                                                                        new [] { 0x0762, 0x14 },
                                                                        new [] { 0x0255, 0x15 },
                                                                        new [] { 0x0D0C, 0x16 },
                                                                        new [] { 0x083B, 0x17 },
                                                                        new [] { 0x355F, 0x18 },
                                                                        new [] { 0x3068, 0x19 },
                                                                        new [] { 0x3F31, 0x1A },
                                                                        new [] { 0x3A06, 0x1B },
                                                                        new [] { 0x24B4, 0x1C },
                                                                        new [] { 0x2183, 0x1D },
                                                                        new [] { 0x2EDA, 0x1E },
                                                                        new [] { 0x2BED, 0x1F }
                                                                       };

       /// <summary> Offset i holds the number of 1 bits in the binary representation of i</summary>
       private static readonly int[] BITS_SET_IN_HALF_BYTE = new[] { 0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4 };

       private readonly ErrorCorrectionLevel errorCorrectionLevel;
       private readonly byte dataMask;

       private FormatInformation(int formatInfo)
       {
           // Bits 3,4
           errorCorrectionLevel = ErrorCorrectionLevel.forBits((formatInfo >> 3) & 0x03);
           // Bottom 3 bits
           dataMask = (byte)(formatInfo & 0x07);
       }

       internal static int numBitsDiffering(int a, int b)
       {
           a ^= b; // a now has a 1 bit exactly where its bit differs with b's
           // Count bits set quickly with a series of lookups:
           return BITS_SET_IN_HALF_BYTE[a & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 4)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 8)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 12)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 16)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 20)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 24)) & 0x0F] +
              BITS_SET_IN_HALF_BYTE[((int)((uint)a >> 28)) & 0x0F];
       }

       /// <summary>
       /// Decodes the format information.
       /// </summary>
       /// <param name="maskedFormatInfo1">format info indicator, with mask still applied</param>
       /// <param name="maskedFormatInfo2">The masked format info2.</param>
       /// <returns>
       /// information about the format it specifies, or <code>null</code>
       /// if doesn't seem to match any known pattern
       /// </returns>
       internal static FormatInformation decodeFormatInformation(int maskedFormatInfo1, int maskedFormatInfo2)
       {
           FormatInformation formatInfo = doDecodeFormatInformation(maskedFormatInfo1, maskedFormatInfo2);
           if (formatInfo != null)
           {
               return formatInfo;
           }
           // Should return null, but, some QR codes apparently
           // do not mask this info. Try again by actually masking the pattern
           // first
           return doDecodeFormatInformation(maskedFormatInfo1 ^ FORMAT_INFO_MASK_QR,
                                            maskedFormatInfo2 ^ FORMAT_INFO_MASK_QR);
       }

       private static FormatInformation doDecodeFormatInformation(int maskedFormatInfo1, int maskedFormatInfo2)
       {
           // Find the int in FORMAT_INFO_DECODE_LOOKUP with fewest bits differing
           int bestDifference = Int32.MaxValue;
           int bestFormatInfo = 0;
           foreach (var decodeInfo in FORMAT_INFO_DECODE_LOOKUP)
           {
               int targetInfo = decodeInfo[0];
               if (targetInfo == maskedFormatInfo1 || targetInfo == maskedFormatInfo2)
               {
                   // Found an exact match
                   return new FormatInformation(decodeInfo[1]);
               }
               int bitsDifference = numBitsDiffering(maskedFormatInfo1, targetInfo);
               if (bitsDifference < bestDifference)
               {
                   bestFormatInfo = decodeInfo[1];
                   bestDifference = bitsDifference;
               }
               if (maskedFormatInfo1 != maskedFormatInfo2)
               {
                   // also try the other option
                   bitsDifference = numBitsDiffering(maskedFormatInfo2, targetInfo);
                   if (bitsDifference < bestDifference)
                   {
                       bestFormatInfo = decodeInfo[1];
                       bestDifference = bitsDifference;
                   }
               }
           }
           // Hamming distance of the 32 masked codes is 7, by construction, so <= 3 bits
           // differing means we found a match
           if (bestDifference <= 3)
           {
               return new FormatInformation(bestFormatInfo);
           }
           return null;
       }

       internal ErrorCorrectionLevel ErrorCorrectionLevel
       {
           get
           {
               return errorCorrectionLevel;
           }
       }

       internal byte DataMask
       {
           get
           {
               return dataMask;
           }
       }

       public override int GetHashCode()
       {
           return (errorCorrectionLevel.ordinal() << 3) | dataMask;
       }

       public override bool Equals(Object o)
       {
           if (!(o is FormatInformation))
           {
               return false;
           }
           var other = (FormatInformation)o;
           return errorCorrectionLevel == other.errorCorrectionLevel && dataMask == other.dataMask;
       }
   }
   public sealed class Mode
   {
       /// <summary>
       /// Gets the name.
       /// </summary>
       public Names Name { get; set; }

       /// <summary>
       /// enumeration for encoding modes
       /// </summary>
       public enum Names
       {
           /// <summary>
           /// 
           /// </summary>
           TERMINATOR,
           /// <summary>
           /// numeric encoding
           /// </summary>
           NUMERIC,
           /// <summary>
           /// alpha-numeric encoding
           /// </summary>
           ALPHANUMERIC,
           /// <summary>
           /// structured append
           /// </summary>
           STRUCTURED_APPEND,
           /// <summary>
           /// byte mode encoding
           /// </summary>
           BYTE,
           /// <summary>
           /// ECI segment
           /// </summary>
           ECI,
           /// <summary>
           /// Kanji mode
           /// </summary>
           KANJI,
           /// <summary>
           /// FNC1 char, first position
           /// </summary>
           FNC1_FIRST_POSITION,
           /// <summary>
           /// FNC1 char, second position
           /// </summary>
           FNC1_SECOND_POSITION,
           /// <summary>
           /// Hanzi mode
           /// </summary>
           HANZI
       }

       // No, we can't use an enum here. J2ME doesn't support it.

       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode TERMINATOR = new Mode(new int[] { 0, 0, 0 }, 0x00, Names.TERMINATOR); // Not really a mode...
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode NUMERIC = new Mode(new int[] { 10, 12, 14 }, 0x01, Names.NUMERIC);
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode ALPHANUMERIC = new Mode(new int[] { 9, 11, 13 }, 0x02, Names.ALPHANUMERIC);
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode STRUCTURED_APPEND = new Mode(new int[] { 0, 0, 0 }, 0x03, Names.STRUCTURED_APPEND); // Not supported
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode BYTE = new Mode(new int[] { 8, 16, 16 }, 0x04, Names.BYTE);
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode ECI = new Mode(null, 0x07, Names.ECI); // character counts don't apply
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode KANJI = new Mode(new int[] { 8, 10, 12 }, 0x08, Names.KANJI);
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode FNC1_FIRST_POSITION = new Mode(null, 0x05, Names.FNC1_FIRST_POSITION);
       /// <summary>
       /// 
       /// </summary>
       public static readonly Mode FNC1_SECOND_POSITION = new Mode(null, 0x09, Names.FNC1_SECOND_POSITION);
       /// <summary>See GBT 18284-2000; "Hanzi" is a transliteration of this mode name.</summary>
       public static readonly Mode HANZI = new Mode(new int[] { 8, 10, 12 }, 0x0D, Names.HANZI);

       private readonly int[] characterCountBitsForVersions;

       private Mode(int[] characterCountBitsForVersions, int bits, Names name)
       {
           this.characterCountBitsForVersions = characterCountBitsForVersions;
           Bits = bits;
           Name = name;
       }

       /// <summary>
       /// Fors the bits.
       /// </summary>
       /// <param name="bits">four bits encoding a QR Code data mode</param>
       /// <returns>
       ///   <see cref="Mode"/> encoded by these bits
       /// </returns>
       /// <exception cref="ArgumentException">if bits do not correspond to a known mode</exception>
       public static Mode forBits(int bits)
       {
           switch (bits)
           {
               case 0x0:
                   return TERMINATOR;
               case 0x1:
                   return NUMERIC;
               case 0x2:
                   return ALPHANUMERIC;
               case 0x3:
                   return STRUCTURED_APPEND;
               case 0x4:
                   return BYTE;
               case 0x5:
                   return FNC1_FIRST_POSITION;
               case 0x7:
                   return ECI;
               case 0x8:
                   return KANJI;
               case 0x9:
                   return FNC1_SECOND_POSITION;
               case 0xD:
                   // 0xD is defined in GBT 18284-2000, may not be supported in foreign country
                   return HANZI;
               default:
                   throw new ArgumentException();
           }
       }

       /// <param name="version">version in question
       /// </param>
       /// <returns> number of bits used, in this QR Code symbol {@link Version}, to encode the
       /// count of characters that will follow encoded in this {@link Mode}
       /// </returns>
       public int getCharacterCountBits(QRVersion version)
       {
           if (characterCountBitsForVersions == null)
           {
               throw new ArgumentException("Character count doesn't apply to this mode");
           }
           int number = version.VersionNumber;
           int offset;
           if (number <= 9)
           {
               offset = 0;
           }
           else if (number <= 26)
           {
               offset = 1;
           }
           else
           {
               offset = 2;
           }
           return characterCountBitsForVersions[offset];
       }

       /// <summary>
       /// Gets the bits.
       /// </summary>
       public int Bits { get; set; }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {
           return Name.ToString();
       }
   }
   public sealed class QRCode
   {
       /// <summary>
       /// 
       /// </summary>
       public static int NUM_MASK_PATTERNS = 8;

       /// <summary>
       /// Initializes a new instance of the <see cref="QRCode"/> class.
       /// </summary>
       public QRCode()
       {
           MaskPattern = -1;
       }

       /// <summary>
       /// Gets or sets the mode.
       /// </summary>
       /// <value>
       /// The mode.
       /// </value>
       public Mode Mode { get; set; }

       /// <summary>
       /// Gets or sets the EC level.
       /// </summary>
       /// <value>
       /// The EC level.
       /// </value>
       public ErrorCorrectionLevel ECLevel { get; set; }

       /// <summary>
       /// Gets or sets the version.
       /// </summary>
       /// <value>
       /// The version.
       /// </value>
       public QRVersion Version { get; set; }

       /// <summary>
       /// Gets or sets the mask pattern.
       /// </summary>
       /// <value>
       /// The mask pattern.
       /// </value>
       public int MaskPattern { get; set; }

       /// <summary>
       /// Gets or sets the matrix.
       /// </summary>
       /// <value>
       /// The matrix.
       /// </value>
       public ByteMatrix Matrix { get; set; }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {
           var result = new StringBuilder(200);
           result.Append("<<\n");
           result.Append(" mode: ");
           result.Append(Mode);
           result.Append("\n ecLevel: ");
           result.Append(ECLevel);
           result.Append("\n version: ");
           if (Version == null)
               result.Append("null");
           else
               result.Append(Version);
           result.Append("\n maskPattern: ");
           result.Append(MaskPattern);
           if (Matrix == null)
           {
               result.Append("\n matrix: null\n");
           }
           else
           {
               result.Append("\n matrix:\n");
               result.Append(Matrix.ToString());
           }
           result.Append(">>\n");
           return result.ToString();
       }

       /// <summary>
       /// Check if "mask_pattern" is valid.
       /// </summary>
       /// <param name="maskPattern">The mask pattern.</param>
       /// <returns>
       ///   <c>true</c> if [is valid mask pattern] [the specified mask pattern]; otherwise, <c>false</c>.
       /// </returns>
       public static bool isValidMaskPattern(int maskPattern)
       {
           return maskPattern >= 0 && maskPattern < NUM_MASK_PATTERNS;
       }
   }
   [System.Flags]
   public enum BarcodeFormat
   {
       /// <summary>Code 39 1D format.</summary>
       CODE_39 = 4,

       /// <summary>Code 93 1D format.</summary>
       CODE_93 = 8,

       /// <summary>Code 128 1D format.</summary>
       CODE_128 = 16,

       /// <summary>QR Code 2D barcode format.</summary>
       QR_CODE = 2048,

   }
   public sealed class ErrorCorrectionLevel
   {
       /// <summary> L = ~7% correction</summary>
       public static readonly ErrorCorrectionLevel L = new ErrorCorrectionLevel(0, 0x01, "L");
       /// <summary> M = ~15% correction</summary>
       public static readonly ErrorCorrectionLevel M = new ErrorCorrectionLevel(1, 0x00, "M");
       /// <summary> Q = ~25% correction</summary>
       public static readonly ErrorCorrectionLevel Q = new ErrorCorrectionLevel(2, 0x03, "Q");
       /// <summary> H = ~30% correction</summary>
       public static readonly ErrorCorrectionLevel H = new ErrorCorrectionLevel(3, 0x02, "H");

       private static readonly ErrorCorrectionLevel[] FOR_BITS = new[] { M, L, H, Q };

       private readonly int bits;

       private ErrorCorrectionLevel(int ordinal, int bits, String name)
       {
           this.ordinal_Renamed_Field = ordinal;
           this.bits = bits;
           this.name = name;
       }

       /// <summary>
       /// Gets the bits.
       /// </summary>
       public int Bits
       {
           get
           {
               return bits;
           }
       }

       /// <summary>
       /// Gets the name.
       /// </summary>
       public String Name
       {
           get
           {
               return name;
           }
       }

       private readonly int ordinal_Renamed_Field;
       private readonly String name;

       /// <summary>
       /// Ordinals this instance.
       /// </summary>
       /// <returns></returns>
       public int ordinal()
       {
           return ordinal_Renamed_Field;
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {
           return name;
       }

       /// <summary>
       /// Fors the bits.
       /// </summary>
       /// <param name="bits">int containing the two bits encoding a QR Code's error correction level</param>
       /// <returns>
       ///   <see cref="ErrorCorrectionLevel"/> representing the encoded error correction level
       /// </returns>
       public static ErrorCorrectionLevel forBits(int bits)
       {
           if (bits < 0 || bits >= FOR_BITS.Length)
           {
               throw new ArgumentException();
           }
           return FOR_BITS[bits];
       }
   }
   public interface Writer
   {
       /// <summary>
       /// Encode a barcode using the default settings.
       /// </summary>
       /// <param name="contents">The contents to encode in the barcode</param>
       /// <param name="format">The barcode format to generate</param>
       /// <param name="width">The preferred width in pixels</param>
       /// <param name="height">The preferred height in pixels</param>
       /// <returns> The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)</returns>
       BitMatrix encode(System.String contents, BarcodeFormat format, int width, int height);

       /// <summary> </summary>
       /// <param name="contents">The contents to encode in the barcode</param>
       /// <param name="format">The barcode format to generate</param>
       /// <param name="width">The preferred width in pixels</param>
       /// <param name="height">The preferred height in pixels</param>
       /// <param name="hints">Additional parameters to supply to the encoder</param>
       /// <returns> The generated barcode as a Matrix of unsigned bytes (0 == black, 255 == white)</returns>
       BitMatrix encode(String contents, BarcodeFormat format, int width, int height, IDictionary<EncodeHintType, object> hints);
   }
   public enum EncodeHintType
   {
       /// <summary>
       /// Specifies the width of the barcode image
       /// type: <see cref="System.Int32" />
       /// </summary>
       WIDTH,

       /// <summary>
       /// Specifies the height of the barcode image
       /// type: <see cref="System.Int32" />
       /// </summary>
       HEIGHT,

       /// <summary>
       /// Don't put the content string into the output image.
       /// type: <see cref="System.Boolean" />
       /// </summary>
       PURE_BARCODE,

       /// <summary>
       /// Specifies what degree of error correction to use, for example in QR Codes.
       /// Type depends on the encoder. For example for QR codes it's type
       /// <see cref="ZXing.QrCode.Internal.ErrorCorrectionLevel" />
       /// For Aztec it is of type <see cref="System.Int32" />, representing the minimal percentage of error correction words. 
       /// In all cases, it can also be a <see cref="System.String" /> representation of the desired value as well.
       /// Note: an Aztec symbol should have a minimum of 25% EC words.
       /// For PDF417 it is of type <see cref="ZXing.PDF417.Internal.PDF417ErrorCorrectionLevel"/> or <see cref="System.Int32" /> (between 0 and 8),
       /// </summary>
       ERROR_CORRECTION,

       /// <summary>
       /// Specifies what character encoding to use where applicable.
       /// type: <see cref="System.String" />
       /// </summary>
       CHARACTER_SET,

       /// <summary>
       /// Specifies margin, in pixels, to use when generating the barcode. The meaning can vary
       /// by format; for example it controls margin before and after the barcode horizontally for
       /// most 1D formats.
       /// type: <see cref="System.Int32" />, or <see cref="System.String" /> representation of the integer value
       /// </summary>
       MARGIN,

       /// <summary>
       /// Specifies the aspect ratio to use.  Default is 4.
       /// type: <see cref="ZXing.PDF417.Internal.PDF417AspectRatio" />, or 1-4.
       /// </summary>
       PDF417_ASPECT_RATIO,

       /// <summary>
       /// Specifies whether to use compact mode for PDF417
       /// type: <see cref="System.Boolean" />, or "true" or "false"
       /// <see cref="System.String" /> value
       /// </summary>
       PDF417_COMPACT,

       /// <summary>
       /// Specifies what compaction mode to use for PDF417.
       /// type: <see cref="ZXing.PDF417.Internal.Compaction" /> or <see cref="System.String" /> value of one of its
       /// enum values
       /// </summary>
       PDF417_COMPACTION,

       /// <summary>
       /// Specifies the minimum and maximum number of rows and columns for PDF417.
       /// type: <see cref="ZXing.PDF417.Internal.Dimensions" />
       /// </summary>
       PDF417_DIMENSIONS,

       /// <summary>
       /// Don't append ECI segment.
       /// That is against the specification of QR Code but some
       /// readers have problems if the charset is switched from
       /// ISO-8859-1 (default) to UTF-8 with the necessary ECI segment.
       /// If you set the property to true you can use UTF-8 encoding
       /// and the ECI segment is omitted.
       /// type: <see cref="System.Boolean" />
       /// </summary>
       DISABLE_ECI,

       /// <summary>
       /// Specifies the matrix shape for Data Matrix (type <see cref="ZXing.Datamatrix.Encoder.SymbolShapeHint"/>)
       /// </summary>
       DATA_MATRIX_SHAPE,

       /// <summary>
       /// Specifies a minimum barcode size (type <see cref="ZXing.Dimension"/>). Only applicable to Data Matrix now.
       /// </summary>
       MIN_SIZE,

       /// <summary>
       /// Specifies a maximum barcode size (type <see cref="ZXing.Dimension"/>). Only applicable to Data Matrix now.
       /// </summary>
       MAX_SIZE,

       /// <summary>
       /// if true, don't switch to codeset C for numbers
       /// </summary>
       CODE128_FORCE_CODESET_B,

       /// <summary>
       /// Specifies the default encodation for Data Matrix (type <see cref="ZXing.Datamatrix.Encoder.Encodation"/>)
       /// Make sure that the content fits into the encodation value, otherwise there will be an exception thrown.
       /// standard value: Encodation.ASCII
       /// </summary>
       DATA_MATRIX_DEFAULT_ENCODATION,

       /// <summary>
       /// Specifies the required number of layers for an Aztec code.
       /// A negative number (-1, -2, -3, -4) specifies a compact Aztec code
       /// 0 indicates to use the minimum number of layers (the default)
       /// A positive number (1, 2, .. 32) specifies a normal (non-compact) Aztec code
       /// type: <see cref="System.Int32" />, or <see cref="System.String" /> representation of the integer value
       /// </summary>
       AZTEC_LAYERS,

       /// <summary>
       /// Specifies the exact version of QR code to be encoded.
       /// (Type <see cref="System.Int32" />, or <see cref="System.String" /> representation of the integer value).
       /// </summary>
       QR_VERSION,

       /// <summary>
       /// Specifies whether the data should be encoded to the GS1 standard
       /// type: <see cref="System.Boolean" />, or "true" or "false"
       /// <see cref="System.String" /> value
       /// </summary>
       GS1_FORMAT,
   }
   internal sealed class BlockPair
   {
       private readonly byte[] dataBytes;
       private readonly byte[] errorCorrectionBytes;

       public BlockPair(byte[] data, byte[] errorCorrection)
       {
           dataBytes = data;
           errorCorrectionBytes = errorCorrection;
       }

       public byte[] DataBytes
       {
           get { return dataBytes; }
       }

       public byte[] ErrorCorrectionBytes
       {
           get { return errorCorrectionBytes; }
       }
   }
   public sealed partial class BitMatrix
   {
       private readonly int width;
       private readonly int height;
       private readonly int rowSize;
       private readonly int[] bits;

       /// <returns> The width of the matrix
       /// </returns>
       public int Width
       {
           get { return width; }
       }

       /// <returns> The height of the matrix
       /// </returns>
       public int Height
       {
           get { return height; }
       }

       /// <summary> This method is for compatibility with older code. It's only logical to call if the matrix
       /// is square, so I'm throwing if that's not the case.
       /// 
       /// </summary>
       /// <returns> row/column dimension of this matrix
       /// </returns>
       public int Dimension
       {
           get
           {
               if (width != height)
               {
                   throw new ArgumentException("Can't call Dimension on a non-square matrix");
               }
               return width;
           }

       }

       /// <returns>
       /// The rowsize of the matrix
       /// </returns>
       public int RowSize
       {
           get { return rowSize; }
       }

       /// <summary>
       /// Creates an empty square <see cref="BitMatrix"/>.
       /// </summary>
       /// <param name="dimension">height and width</param>
       public BitMatrix(int dimension)
           : this(dimension, dimension)
       {
       }

       /// <summary>
       /// Creates an empty square <see cref="BitMatrix"/>.
       /// </summary>
       /// <param name="width">bit matrix width</param>
       /// <param name="height">bit matrix height</param>
       public BitMatrix(int width, int height)
       {
           if (width < 1 || height < 1)
           {
               throw new System.ArgumentException("Both dimensions must be greater than 0");
           }
           this.width = width;
           this.height = height;
           this.rowSize = (width + 31) >> 5;
           bits = new int[rowSize * height];
       }

       internal BitMatrix(int width, int height, int rowSize, int[] bits)
       {
           this.width = width;
           this.height = height;
           this.rowSize = rowSize;
           this.bits = bits;
       }

       internal BitMatrix(int width, int height, int[] bits)
       {
           this.width = width;
           this.height = height;
           this.rowSize = (width + 31) >> 5;
           this.bits = bits;
       }

       /// <summary>
       /// Interprets a 2D array of booleans as a <see cref="BitMatrix"/>, where "true" means an "on" bit.
       /// </summary>
       /// <param name="image">bits of the image, as a row-major 2D array. Elements are arrays representing rows</param>
       /// <returns><see cref="BitMatrix"/> representation of image</returns>
       public static BitMatrix parse(bool[][] image)
       {
           var height = image.Length;
           var width = image[0].Length;
           var bits = new BitMatrix(width, height);
           for (var i = 0; i < height; i++)
           {
               var imageI = image[i];
               for (var j = 0; j < width; j++)
               {
                   bits[j, i] = imageI[j];
               }
           }
           return bits;
       }

       public static BitMatrix parse(String stringRepresentation, String setString, String unsetString)
       {
           if (stringRepresentation == null)
           {
               throw new ArgumentException();
           }

           bool[] bits = new bool[stringRepresentation.Length];
           int bitsPos = 0;
           int rowStartPos = 0;
           int rowLength = -1;
           int nRows = 0;
           int pos = 0;
           while (pos < stringRepresentation.Length)
           {
               if (stringRepresentation.Substring(pos, 1).Equals("\n") ||
                   stringRepresentation.Substring(pos, 1).Equals("\r"))
               {
                   if (bitsPos > rowStartPos)
                   {
                       if (rowLength == -1)
                       {
                           rowLength = bitsPos - rowStartPos;
                       }
                       else if (bitsPos - rowStartPos != rowLength)
                       {
                           throw new ArgumentException("row lengths do not match");
                       }
                       rowStartPos = bitsPos;
                       nRows++;
                   }
                   pos++;
               }
               else if (stringRepresentation.Substring(pos, setString.Length).Equals(setString))
               {
                   pos += setString.Length;
                   bits[bitsPos] = true;
                   bitsPos++;
               }
               else if (stringRepresentation.Substring(pos, unsetString.Length).Equals(unsetString))
               {
                   pos += unsetString.Length;
                   bits[bitsPos] = false;
                   bitsPos++;
               }
               else
               {
                   throw new ArgumentException("illegal character encountered: " + stringRepresentation.Substring(pos));
               }
           }

           // no EOL at end?
           if (bitsPos > rowStartPos)
           {
               if (rowLength == -1)
               {
                   rowLength = bitsPos - rowStartPos;
               }
               else if (bitsPos - rowStartPos != rowLength)
               {
                   throw new ArgumentException("row lengths do not match");
               }
               nRows++;
           }

           BitMatrix matrix = new BitMatrix(rowLength, nRows);
           for (int i = 0; i < bitsPos; i++)
           {
               if (bits[i])
               {
                   matrix[i % rowLength, i / rowLength] = true;
               }
           }
           return matrix;
       }

       /// <summary> <p>Gets the requested bit, where true means black.</p>
       /// 
       /// </summary>
       /// <param name="x">The horizontal component (i.e. which column)
       /// </param>
       /// <param name="y">The vertical component (i.e. which row)
       /// </param>
       /// <returns> value of given bit in matrix
       /// </returns>
       public bool this[int x, int y]
       {
           get
           {
               int offset = y * rowSize + (x >> 5);
               return (((int)((uint)(bits[offset]) >> (x & 0x1f))) & 1) != 0;
           }
           set
           {
               if (value)
               {
                   int offset = y * rowSize + (x >> 5);
                   bits[offset] |= 1 << (x & 0x1f);
               }
               else
               {
                   int offset = y * rowSize + (x / 32);
                   bits[offset] &= ~(1 << (x & 0x1f));
               }
           }
       }

       /// <summary>
       /// <p>Flips the given bit.</p>
       /// </summary>
       /// <param name="x">The horizontal component (i.e. which column)</param>
       /// <param name="y">The vertical component (i.e. which row)</param>
       public void flip(int x, int y)
       {
           int offset = y * rowSize + (x >> 5);
           bits[offset] ^= 1 << (x & 0x1f);
       }

       /// <summary>
       /// flip all of the bits, if shouldBeFlipped is true for the coordinates
       /// </summary>
       /// <param name="shouldBeFlipped">should return true, if the bit at a given coordinate should be flipped</param>
       public void flipWhen(Func<int, int, bool> shouldBeFlipped)
       {
           for (var y = 0; y < height; y++)
           {
               for (var x = 0; x < width; x++)
               {
                   if (shouldBeFlipped(y, x))
                   {
                       int offset = y * rowSize + (x >> 5);
                       bits[offset] ^= 1 << (x & 0x1f);
                   }
               }
           }
       }

       /// <summary>
       /// Exclusive-or (XOR): Flip the bit in this {@code BitMatrix} if the corresponding
       /// mask bit is set.
       /// </summary>
       /// <param name="mask">The mask.</param>
       public void xor(BitMatrix mask)
       {
           if (width != mask.Width || height != mask.Height
               || rowSize != mask.RowSize)
           {
               throw new ArgumentException("input matrix dimensions do not match");
           }
           var rowArray = new BitArray(width / 32 + 1);
           for (int y = 0; y < height; y++)
           {
               int offset = y * rowSize;
               int[] row = mask.getRow(y, rowArray).Array;
               for (int x = 0; x < rowSize; x++)
               {
                   bits[offset + x] ^= row[x];
               }
           }
       }

       /// <summary> Clears all bits (sets to false).</summary>
       public void clear()
       {
           int max = bits.Length;
           for (int i = 0; i < max; i++)
           {
               bits[i] = 0;
           }
       }

       /// <summary> <p>Sets a square region of the bit matrix to true.</p>
       /// 
       /// </summary>
       /// <param name="left">The horizontal position to begin at (inclusive)
       /// </param>
       /// <param name="top">The vertical position to begin at (inclusive)
       /// </param>
       /// <param name="width">The width of the region
       /// </param>
       /// <param name="height">The height of the region
       /// </param>
       public void setRegion(int left, int top, int width, int height)
       {
           if (top < 0 || left < 0)
           {
               throw new System.ArgumentException("Left and top must be nonnegative");
           }
           if (height < 1 || width < 1)
           {
               throw new System.ArgumentException("Height and width must be at least 1");
           }
           int right = left + width;
           int bottom = top + height;
           if (bottom > this.height || right > this.width)
           {
               throw new System.ArgumentException("The region must fit inside the matrix");
           }
           for (int y = top; y < bottom; y++)
           {
               int offset = y * rowSize;
               for (int x = left; x < right; x++)
               {
                   bits[offset + (x >> 5)] |= 1 << (x & 0x1f);
               }
           }
       }

       /// <summary> A fast method to retrieve one row of data from the matrix as a BitArray.
       /// 
       /// </summary>
       /// <param name="y">The row to retrieve
       /// </param>
       /// <param name="row">An optional caller-allocated BitArray, will be allocated if null or too small
       /// </param>
       /// <returns> The resulting BitArray - this reference should always be used even when passing
       /// your own row
       /// </returns>
       public BitArray getRow(int y, BitArray row)
       {
           if (row == null || row.Size < width)
           {
               row = new BitArray(width);
           }
           else
           {
               row.clear();
           }
           int offset = y * rowSize;
           for (int x = 0; x < rowSize; x++)
           {
               row.setBulk(x << 5, bits[offset + x]);
           }
           return row;
       }

       /// <summary>
       /// Sets the row.
       /// </summary>
       /// <param name="y">row to set</param>
       /// <param name="row">{@link BitArray} to copy from</param>
       public void setRow(int y, BitArray row)
       {
           Array.Copy(row.Array, 0, bits, y * rowSize, rowSize);
       }

       /// <summary>
       /// Modifies this {@code BitMatrix} to represent the same but rotated 180 degrees
       /// </summary>
       public void rotate180()
       {
           var width = Width;
           var height = Height;
           var topRow = new BitArray(width);
           var bottomRow = new BitArray(width);
           for (int i = 0; i < (height + 1) / 2; i++)
           {
               topRow = getRow(i, topRow);
               bottomRow = getRow(height - 1 - i, bottomRow);
               topRow.reverse();
               bottomRow.reverse();
               setRow(i, bottomRow);
               setRow(height - 1 - i, topRow);
           }
       }

       /// <summary>
       /// This is useful in detecting the enclosing rectangle of a 'pure' barcode.
       /// </summary>
       /// <returns>{left,top,width,height} enclosing rectangle of all 1 bits, or null if it is all white</returns>
       public int[] getEnclosingRectangle()
       {
           int left = width;
           int top = height;
           int right = -1;
           int bottom = -1;

           for (int y = 0; y < height; y++)
           {
               for (int x32 = 0; x32 < rowSize; x32++)
               {
                   int theBits = bits[y * rowSize + x32];
                   if (theBits != 0)
                   {
                       if (y < top)
                       {
                           top = y;
                       }
                       if (y > bottom)
                       {
                           bottom = y;
                       }
                       if (x32 * 32 < left)
                       {
                           int bit = 0;
                           while ((theBits << (31 - bit)) == 0)
                           {
                               bit++;
                           }
                           if ((x32 * 32 + bit) < left)
                           {
                               left = x32 * 32 + bit;
                           }
                       }
                       if (x32 * 32 + 31 > right)
                       {
                           int bit = 31;
                           while (((int)((uint)theBits >> bit)) == 0) // (theBits >>> bit)
                           {
                               bit--;
                           }
                           if ((x32 * 32 + bit) > right)
                           {
                               right = x32 * 32 + bit;
                           }
                       }
                   }
               }
           }

           if (right < left || bottom < top)
           {
               return null;
           }

           return new[] { left, top, right - left + 1, bottom - top + 1 };
       }

       /// <summary>
       /// This is useful in detecting a corner of a 'pure' barcode.
       /// </summary>
       /// <returns>{x,y} coordinate of top-left-most 1 bit, or null if it is all white</returns>
       public int[] getTopLeftOnBit()
       {
           int bitsOffset = 0;
           while (bitsOffset < bits.Length && bits[bitsOffset] == 0)
           {
               bitsOffset++;
           }
           if (bitsOffset == bits.Length)
           {
               return null;
           }
           int y = bitsOffset / rowSize;
           int x = (bitsOffset % rowSize) << 5;

           int theBits = bits[bitsOffset];
           int bit = 0;
           while ((theBits << (31 - bit)) == 0)
           {
               bit++;
           }
           x += bit;
           return new[] { x, y };
       }

       public int[] getBottomRightOnBit()
       {
           int bitsOffset = bits.Length - 1;
           while (bitsOffset >= 0 && bits[bitsOffset] == 0)
           {
               bitsOffset--;
           }
           if (bitsOffset < 0)
           {
               return null;
           }

           int y = bitsOffset / rowSize;
           int x = (bitsOffset % rowSize) << 5;

           int theBits = bits[bitsOffset];
           int bit = 31;

           while (((int)((uint)theBits >> bit)) == 0) // (theBits >>> bit)
           {
               bit--;
           }
           x += bit;

           return new int[] { x, y };
       }

       /// <summary>
       /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
       /// </summary>
       /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
       /// <returns>
       ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
       /// </returns>
       public override bool Equals(object obj)
       {
           if (!(obj is BitMatrix))
           {
               return false;
           }
           var other = (BitMatrix)obj;
           if (width != other.width || height != other.height ||
               rowSize != other.rowSize || bits.Length != other.bits.Length)
           {
               return false;
           }
           for (int i = 0; i < bits.Length; i++)
           {
               if (bits[i] != other.bits[i])
               {
                   return false;
               }
           }
           return true;
       }

       /// <summary>
       /// Returns a hash code for this instance.
       /// </summary>
       /// <returns>
       /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
       /// </returns>
       public override int GetHashCode()
       {
           int hash = width;
           hash = 31 * hash + width;
           hash = 31 * hash + height;
           hash = 31 * hash + rowSize;
           foreach (var bit in bits)
           {
               hash = 31 * hash + bit.GetHashCode();
           }
           return hash;
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {

           return ToString("X ", "  ", Environment.NewLine);

       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <param name="setString">The set string.</param>
       /// <param name="unsetString">The unset string.</param>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public String ToString(String setString, String unsetString)
       {

           return buildToString(setString, unsetString, Environment.NewLine);

       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <param name="setString">The set string.</param>
       /// <param name="unsetString">The unset string.</param>
       /// <param name="lineSeparator">The line separator.</param>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public String ToString(String setString, String unsetString, String lineSeparator)
       {
           return buildToString(setString, unsetString, lineSeparator);
       }

       private String buildToString(String setString, String unsetString, String lineSeparator)
       {
           var result = new StringBuilder(height * (width + 1));
           for (int y = 0; y < height; y++)
           {
               for (int x = 0; x < width; x++)
               {
                   result.Append(this[x, y] ? setString : unsetString);
               }
               result.Append(lineSeparator);
           }
           return result.ToString();
       }

       /// <summary>
       /// Clones this instance.
       /// </summary>
       /// <returns></returns>
       public object Clone()
       {
           return new BitMatrix(width, height, rowSize, (int[])bits.Clone());
       }


   }
   internal class TimeZoneInfo
   {
       internal static TimeZoneInfo Local = null;

       internal static DateTime ConvertTime(DateTime dateTime, TimeZoneInfo destinationTimeZone)
       {
           // TODO: fix it for .net 2.0
           return dateTime;
       }
   }
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="TResult">The type of the result.</typeparam>
   /// <returns></returns>
   public delegate TResult Func<out TResult>();
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="TResult">The type of the result.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <returns></returns>
   public delegate TResult Func<in T1, out TResult>(T1 param1);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <typeparam name="TResult">The type of the result.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   /// <returns></returns>
   public delegate TResult Func<in T1, in T2, out TResult>(T1 param1, T2 param2);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <typeparam name="T3">The type of the 3.</typeparam>
   /// <typeparam name="TResult">The type of the result.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   /// <param name="param3">The param3.</param>
   /// <returns></returns>
   public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 param1, T2 param2, T3 param3);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <typeparam name="T3">The type of the 3.</typeparam>
   /// <typeparam name="T4">The type of the 4.</typeparam>
   /// <typeparam name="TResult">The type of the result.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   /// <param name="param3">The param3.</param>
   /// <param name="param4">The param4.</param>
   /// <returns></returns>
   public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 param1, T2 param2, T3 param3, T4 param4);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   public delegate void Action();
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <param name="param1">The param1.</param>
   public delegate void Action<in T1>(T1 param1);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   public delegate void Action<in T1, in T2>(T1 param1, T2 param2);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <typeparam name="T3">The type of the 3.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   /// <param name="param3">The param3.</param>
   public delegate void Action<in T1, in T2, in T3>(T1 param1, T2 param2, T3 param3);
   /// <summary>
   /// for compatibility with .net 4.0
   /// </summary>
   /// <typeparam name="T1">The type of the 1.</typeparam>
   /// <typeparam name="T2">The type of the 2.</typeparam>
   /// <typeparam name="T3">The type of the 3.</typeparam>
   /// <typeparam name="T4">The type of the 4.</typeparam>
   /// <param name="param1">The param1.</param>
   /// <param name="param2">The param2.</param>
   /// <param name="param3">The param3.</param>
   /// <param name="param4">The param4.</param>
   public delegate void Action<in T1, in T2, in T3, in T4>(T1 param1, T2 param2, T3 param3, T4 param4);
   public sealed class QRVersion
   {
       /// <summary> See ISO 18004:2006 Annex D.
       /// Element i represents the raw version bits that specify version i + 7
       /// </summary>
       private static readonly int[] VERSION_DECODE_INFO = new[]
                                                               {
                                                                0x07C94, 0x085BC, 0x09A99, 0x0A4D3, 0x0BBF6,
                                                                0x0C762, 0x0D847, 0x0E60D, 0x0F928, 0x10B78,
                                                                0x1145D, 0x12A17, 0x13532, 0x149A6, 0x15683,
                                                                0x168C9, 0x177EC, 0x18EC4, 0x191E1, 0x1AFAB,
                                                                0x1B08E, 0x1CC1A, 0x1D33F, 0x1ED75, 0x1F250,
                                                                0x209D5, 0x216F0, 0x228BA, 0x2379F, 0x24B0B,
                                                                0x2542E, 0x26A64, 0x27541, 0x28C69
                                                             };

       private static readonly QRVersion[] VERSIONS = buildVersions();

       private readonly int versionNumber;
       private readonly int[] alignmentPatternCenters;
       private readonly ECBlocks[] ecBlocks;
       private readonly int totalCodewords;

       private QRVersion(int versionNumber, int[] alignmentPatternCenters, params ECBlocks[] ecBlocks)
       {
           this.versionNumber = versionNumber;
           this.alignmentPatternCenters = alignmentPatternCenters;
           this.ecBlocks = ecBlocks;
           int total = 0;
           int ecCodewords = ecBlocks[0].ECCodewordsPerBlock;
           ECB[] ecbArray = ecBlocks[0].getECBlocks();
           foreach (var ecBlock in ecbArray)
           {
               total += ecBlock.Count * (ecBlock.DataCodewords + ecCodewords);
           }
           this.totalCodewords = total;
       }

       /// <summary>
       /// Gets the version number.
       /// </summary>
       public int VersionNumber
       {
           get
           {
               return versionNumber;
           }

       }

       /// <summary>
       /// Gets the alignment pattern centers.
       /// </summary>
       public int[] AlignmentPatternCenters
       {
           get
           {
               return alignmentPatternCenters;
           }

       }

       /// <summary>
       /// Gets the total codewords.
       /// </summary>
       public int TotalCodewords
       {
           get
           {
               return totalCodewords;
           }

       }

       /// <summary>
       /// Gets the dimension for version.
       /// </summary>
       public int DimensionForVersion
       {
           get
           {
               return 17 + 4 * versionNumber;
           }

       }

       /// <summary>
       /// Gets the EC blocks for level.
       /// </summary>
       /// <param name="ecLevel">The ec level.</param>
       /// <returns></returns>
       public ECBlocks getECBlocksForLevel(ErrorCorrectionLevel ecLevel)
       {
           return ecBlocks[ecLevel.ordinal()];
       }

       /// <summary> <p>Deduces version information purely from QR Code dimensions.</p>
       /// 
       /// </summary>
       /// <param name="dimension">dimension in modules
       /// </param>
       /// <returns><see cref="Version" /> for a QR Code of that dimension or null</returns>
       public static QRVersion getProvisionalVersionForDimension(int dimension)
       {
           if (dimension % 4 != 1)
           {
               return null;
           }
           try
           {
               return getVersionForNumber((dimension - 17) >> 2);
           }
           catch (ArgumentException)
           {
               return null;
           }
       }

       /// <summary>
       /// Gets the version for number.
       /// </summary>
       /// <param name="versionNumber">The version number.</param>
       /// <returns></returns>
       public static QRVersion getVersionForNumber(int versionNumber)
       {
           if (versionNumber < 1 || versionNumber > 40)
           {
               throw new ArgumentException();
           }
           return VERSIONS[versionNumber - 1];
       }

       internal static QRVersion decodeVersionInformation(int versionBits)
       {
           int bestDifference = Int32.MaxValue;
           int bestVersion = 0;
           for (int i = 0; i < VERSION_DECODE_INFO.Length; i++)
           {
               int targetVersion = VERSION_DECODE_INFO[i];
               // Do the version info bits match exactly? done.
               if (targetVersion == versionBits)
               {
                   return getVersionForNumber(i + 7);
               }
               // Otherwise see if this is the closest to a real version info bit string
               // we have seen so far
               int bitsDifference = FormatInformation.numBitsDiffering(versionBits, targetVersion);
               if (bitsDifference < bestDifference)
               {
                   bestVersion = i + 7;
                   bestDifference = bitsDifference;
               }
           }
           // We can tolerate up to 3 bits of error since no two version info codewords will
           // differ in less than 8 bits.
           if (bestDifference <= 3)
           {
               return getVersionForNumber(bestVersion);
           }
           // If we didn't find a close enough match, fail
           return null;
       }

       /// <summary> See ISO 18004:2006 Annex E</summary>
       internal BitMatrix buildFunctionPattern()
       {
           int dimension = DimensionForVersion;
           BitMatrix bitMatrix = new BitMatrix(dimension);

           // Top left finder pattern + separator + format
           bitMatrix.setRegion(0, 0, 9, 9);
           // Top right finder pattern + separator + format
           bitMatrix.setRegion(dimension - 8, 0, 8, 9);
           // Bottom left finder pattern + separator + format
           bitMatrix.setRegion(0, dimension - 8, 9, 8);

           // Alignment patterns
           int max = alignmentPatternCenters.Length;
           for (int x = 0; x < max; x++)
           {
               int i = alignmentPatternCenters[x] - 2;
               for (int y = 0; y < max; y++)
               {
                   if ((x == 0 && (y == 0 || y == max - 1)) || (x == max - 1 && y == 0))
                   {
                       // No alignment patterns near the three finder patterns
                       continue;
                   }
                   bitMatrix.setRegion(alignmentPatternCenters[y] - 2, i, 5, 5);
               }
           }

           // Vertical timing pattern
           bitMatrix.setRegion(6, 9, 1, dimension - 17);
           // Horizontal timing pattern
           bitMatrix.setRegion(9, 6, dimension - 17, 1);

           if (versionNumber > 6)
           {
               // Version info, top right
               bitMatrix.setRegion(dimension - 11, 0, 3, 6);
               // Version info, bottom left
               bitMatrix.setRegion(0, dimension - 11, 6, 3);
           }

           return bitMatrix;
       }

       /// <summary> <p>Encapsulates a set of error-correction blocks in one symbol version. Most versions will
       /// use blocks of differing sizes within one version, so, this encapsulates the parameters for
       /// each set of blocks. It also holds the number of error-correction codewords per block since it
       /// will be the same across all blocks within one version.</p>
       /// </summary>
       public sealed class ECBlocks
       {
           private readonly int ecCodewordsPerBlock;
           private readonly ECB[] ecBlocks;

           internal ECBlocks(int ecCodewordsPerBlock, params ECB[] ecBlocks)
           {
               this.ecCodewordsPerBlock = ecCodewordsPerBlock;
               this.ecBlocks = ecBlocks;
           }

           /// <summary>
           /// Gets the EC codewords per block.
           /// </summary>
           public int ECCodewordsPerBlock
           {
               get
               {
                   return ecCodewordsPerBlock;
               }
           }

           /// <summary>
           /// Gets the num blocks.
           /// </summary>
           public int NumBlocks
           {
               get
               {
                   int total = 0;
                   foreach (var ecBlock in ecBlocks)
                   {
                       total += ecBlock.Count;
                   }
                   return total;
               }
           }

           /// <summary>
           /// Gets the total EC codewords.
           /// </summary>
           public int TotalECCodewords
           {
               get
               {
                   return ecCodewordsPerBlock * NumBlocks;
               }
           }

           /// <summary>
           /// Gets the EC blocks.
           /// </summary>
           /// <returns></returns>
           public ECB[] getECBlocks()
           {
               return ecBlocks;
           }
       }

       /// <summary> <p>Encapsulates the parameters for one error-correction block in one symbol version.
       /// This includes the number of data codewords, and the number of times a block with these
       /// parameters is used consecutively in the QR code version's format.</p>
       /// </summary>
       public sealed class ECB
       {
           private readonly int count;
           private readonly int dataCodewords;

           internal ECB(int count, int dataCodewords)
           {
               this.count = count;
               this.dataCodewords = dataCodewords;
           }

           /// <summary>
           /// Gets the count.
           /// </summary>
           public int Count
           {
               get
               {
                   return count;
               }

           }
           /// <summary>
           /// Gets the data codewords.
           /// </summary>
           public int DataCodewords
           {
               get
               {
                   return dataCodewords;
               }

           }
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {
           return Convert.ToString(versionNumber);
       }

       /// <summary> See ISO 18004:2006 6.5.1 Table 9</summary>
       private static QRVersion[] buildVersions()
       {
           return new QRVersion[]
               {
               new QRVersion(1, new int[] {},
                           new ECBlocks(7, new ECB(1, 19)),
                           new ECBlocks(10, new ECB(1, 16)),
                           new ECBlocks(13, new ECB(1, 13)),
                           new ECBlocks(17, new ECB(1, 9))),
               new QRVersion(2, new int[] {6, 18},
                           new ECBlocks(10, new ECB(1, 34)),
                           new ECBlocks(16, new ECB(1, 28)),
                           new ECBlocks(22, new ECB(1, 22)),
                           new ECBlocks(28, new ECB(1, 16))),
               new QRVersion(3, new int[] {6, 22},
                           new ECBlocks(15, new ECB(1, 55)),
                           new ECBlocks(26, new ECB(1, 44)),
                           new ECBlocks(18, new ECB(2, 17)),
                           new ECBlocks(22, new ECB(2, 13))),
               new QRVersion(4, new int[] {6, 26},
                           new ECBlocks(20, new ECB(1, 80)),
                           new ECBlocks(18, new ECB(2, 32)),
                           new ECBlocks(26, new ECB(2, 24)),
                           new ECBlocks(16, new ECB(4, 9))),
               new QRVersion(5, new int[] {6, 30},
                           new ECBlocks(26, new ECB(1, 108)),
                           new ECBlocks(24, new ECB(2, 43)),
                           new ECBlocks(18, new ECB(2, 15),
                                        new ECB(2, 16)),
                           new ECBlocks(22, new ECB(2, 11),
                                        new ECB(2, 12))),
               new QRVersion(6, new int[] {6, 34},
                           new ECBlocks(18, new ECB(2, 68)),
                           new ECBlocks(16, new ECB(4, 27)),
                           new ECBlocks(24, new ECB(4, 19)),
                           new ECBlocks(28, new ECB(4, 15))),
               new QRVersion(7, new int[] {6, 22, 38},
                           new ECBlocks(20, new ECB(2, 78)),
                           new ECBlocks(18, new ECB(4, 31)),
                           new ECBlocks(18, new ECB(2, 14),
                                        new ECB(4, 15)),
                           new ECBlocks(26, new ECB(4, 13),
                                        new ECB(1, 14))),
               new QRVersion(8, new int[] {6, 24, 42},
                           new ECBlocks(24, new ECB(2, 97)),
                           new ECBlocks(22, new ECB(2, 38),
                                        new ECB(2, 39)),
                           new ECBlocks(22, new ECB(4, 18),
                                        new ECB(2, 19)),
                           new ECBlocks(26, new ECB(4, 14),
                                        new ECB(2, 15))),
               new QRVersion(9, new int[] {6, 26, 46},
                           new ECBlocks(30, new ECB(2, 116)),
                           new ECBlocks(22, new ECB(3, 36),
                                        new ECB(2, 37)),
                           new ECBlocks(20, new ECB(4, 16),
                                        new ECB(4, 17)),
                           new ECBlocks(24, new ECB(4, 12),
                                        new ECB(4, 13))),
               new QRVersion(10, new int[] {6, 28, 50},
                           new ECBlocks(18, new ECB(2, 68),
                                        new ECB(2, 69)),
                           new ECBlocks(26, new ECB(4, 43),
                                        new ECB(1, 44)),
                           new ECBlocks(24, new ECB(6, 19),
                                        new ECB(2, 20)),
                           new ECBlocks(28, new ECB(6, 15),
                                        new ECB(2, 16))),
               new QRVersion(11, new int[] {6, 30, 54},
                           new ECBlocks(20, new ECB(4, 81)),
                           new ECBlocks(30, new ECB(1, 50),
                                        new ECB(4, 51)),
                           new ECBlocks(28, new ECB(4, 22),
                                        new ECB(4, 23)),
                           new ECBlocks(24, new ECB(3, 12),
                                        new ECB(8, 13))),
               new QRVersion(12, new int[] {6, 32, 58},
                           new ECBlocks(24, new ECB(2, 92),
                                        new ECB(2, 93)),
                           new ECBlocks(22, new ECB(6, 36),
                                        new ECB(2, 37)),
                           new ECBlocks(26, new ECB(4, 20),
                                        new ECB(6, 21)),
                           new ECBlocks(28, new ECB(7, 14),
                                        new ECB(4, 15))),
               new QRVersion(13, new int[] {6, 34, 62},
                           new ECBlocks(26, new ECB(4, 107)),
                           new ECBlocks(22, new ECB(8, 37),
                                        new ECB(1, 38)),
                           new ECBlocks(24, new ECB(8, 20),
                                        new ECB(4, 21)),
                           new ECBlocks(22, new ECB(12, 11),
                                        new ECB(4, 12))),
               new QRVersion(14, new int[] {6, 26, 46, 66},
                           new ECBlocks(30, new ECB(3, 115),
                                        new ECB(1, 116)),
                           new ECBlocks(24, new ECB(4, 40),
                                        new ECB(5, 41)),
                           new ECBlocks(20, new ECB(11, 16),
                                        new ECB(5, 17)),
                           new ECBlocks(24, new ECB(11, 12),
                                        new ECB(5, 13))),
               new QRVersion(15, new int[] {6, 26, 48, 70},
                           new ECBlocks(22, new ECB(5, 87),
                                        new ECB(1, 88)),
                           new ECBlocks(24, new ECB(5, 41),
                                        new ECB(5, 42)),
                           new ECBlocks(30, new ECB(5, 24),
                                        new ECB(7, 25)),
                           new ECBlocks(24, new ECB(11, 12),
                                        new ECB(7, 13))),
               new QRVersion(16, new int[] {6, 26, 50, 74},
                           new ECBlocks(24, new ECB(5, 98),
                                        new ECB(1, 99)),
                           new ECBlocks(28, new ECB(7, 45),
                                        new ECB(3, 46)),
                           new ECBlocks(24, new ECB(15, 19),
                                        new ECB(2, 20)),
                           new ECBlocks(30, new ECB(3, 15),
                                        new ECB(13, 16))),
               new QRVersion(17, new int[] {6, 30, 54, 78},
                           new ECBlocks(28, new ECB(1, 107),
                                        new ECB(5, 108)),
                           new ECBlocks(28, new ECB(10, 46),
                                        new ECB(1, 47)),
                           new ECBlocks(28, new ECB(1, 22),
                                        new ECB(15, 23)),
                           new ECBlocks(28, new ECB(2, 14),
                                        new ECB(17, 15))),
               new QRVersion(18, new int[] {6, 30, 56, 82},
                           new ECBlocks(30, new ECB(5, 120),
                                        new ECB(1, 121)),
                           new ECBlocks(26, new ECB(9, 43),
                                        new ECB(4, 44)),
                           new ECBlocks(28, new ECB(17, 22),
                                        new ECB(1, 23)),
                           new ECBlocks(28, new ECB(2, 14),
                                        new ECB(19, 15))),
               new QRVersion(19, new int[] {6, 30, 58, 86},
                           new ECBlocks(28, new ECB(3, 113),
                                        new ECB(4, 114)),
                           new ECBlocks(26, new ECB(3, 44),
                                        new ECB(11, 45)),
                           new ECBlocks(26, new ECB(17, 21),
                                        new ECB(4, 22)),
                           new ECBlocks(26, new ECB(9, 13),
                                        new ECB(16, 14))),
               new QRVersion(20, new int[] {6, 34, 62, 90},
                           new ECBlocks(28, new ECB(3, 107),
                                        new ECB(5, 108)),
                           new ECBlocks(26, new ECB(3, 41),
                                        new ECB(13, 42)),
                           new ECBlocks(30, new ECB(15, 24),
                                        new ECB(5, 25)),
                           new ECBlocks(28, new ECB(15, 15),
                                        new ECB(10, 16))),
               new QRVersion(21, new int[] {6, 28, 50, 72, 94},
                           new ECBlocks(28, new ECB(4, 116),
                                        new ECB(4, 117)),
                           new ECBlocks(26, new ECB(17, 42)),
                           new ECBlocks(28, new ECB(17, 22),
                                        new ECB(6, 23)),
                           new ECBlocks(30, new ECB(19, 16),
                                        new ECB(6, 17))),
               new QRVersion(22, new int[] {6, 26, 50, 74, 98},
                           new ECBlocks(28, new ECB(2, 111),
                                        new ECB(7, 112)),
                           new ECBlocks(28, new ECB(17, 46)),
                           new ECBlocks(30, new ECB(7, 24),
                                        new ECB(16, 25)),
                           new ECBlocks(24, new ECB(34, 13))),
               new QRVersion(23, new int[] {6, 30, 54, 78, 102},
                           new ECBlocks(30, new ECB(4, 121),
                                        new ECB(5, 122)),
                           new ECBlocks(28, new ECB(4, 47),
                                        new ECB(14, 48)),
                           new ECBlocks(30, new ECB(11, 24),
                                        new ECB(14, 25)),
                           new ECBlocks(30, new ECB(16, 15),
                                        new ECB(14, 16))),
               new QRVersion(24, new int[] {6, 28, 54, 80, 106},
                           new ECBlocks(30, new ECB(6, 117),
                                        new ECB(4, 118)),
                           new ECBlocks(28, new ECB(6, 45),
                                        new ECB(14, 46)),
                           new ECBlocks(30, new ECB(11, 24),
                                        new ECB(16, 25)),
                           new ECBlocks(30, new ECB(30, 16),
                                        new ECB(2, 17))),
               new QRVersion(25, new int[] {6, 32, 58, 84, 110},
                           new ECBlocks(26, new ECB(8, 106),
                                        new ECB(4, 107)),
                           new ECBlocks(28, new ECB(8, 47),
                                        new ECB(13, 48)),
                           new ECBlocks(30, new ECB(7, 24),
                                        new ECB(22, 25)),
                           new ECBlocks(30, new ECB(22, 15),
                                        new ECB(13, 16))),
               new QRVersion(26, new int[] {6, 30, 58, 86, 114},
                           new ECBlocks(28, new ECB(10, 114),
                                        new ECB(2, 115)),
                           new ECBlocks(28, new ECB(19, 46),
                                        new ECB(4, 47)),
                           new ECBlocks(28, new ECB(28, 22),
                                        new ECB(6, 23)),
                           new ECBlocks(30, new ECB(33, 16),
                                        new ECB(4, 17))),
               new QRVersion(27, new int[] {6, 34, 62, 90, 118},
                           new ECBlocks(30, new ECB(8, 122),
                                        new ECB(4, 123)),
                           new ECBlocks(28, new ECB(22, 45),
                                        new ECB(3, 46)),
                           new ECBlocks(30, new ECB(8, 23),
                                        new ECB(26, 24)),
                           new ECBlocks(30, new ECB(12, 15),
                                        new ECB(28, 16))),
               new QRVersion(28, new int[] {6, 26, 50, 74, 98, 122},
                           new ECBlocks(30, new ECB(3, 117),
                                        new ECB(10, 118)),
                           new ECBlocks(28, new ECB(3, 45),
                                        new ECB(23, 46)),
                           new ECBlocks(30, new ECB(4, 24),
                                        new ECB(31, 25)),
                           new ECBlocks(30, new ECB(11, 15),
                                        new ECB(31, 16))),
               new QRVersion(29, new int[] {6, 30, 54, 78, 102, 126},
                           new ECBlocks(30, new ECB(7, 116),
                                        new ECB(7, 117)),
                           new ECBlocks(28, new ECB(21, 45),
                                        new ECB(7, 46)),
                           new ECBlocks(30, new ECB(1, 23),
                                        new ECB(37, 24)),
                           new ECBlocks(30, new ECB(19, 15),
                                        new ECB(26, 16))),
               new QRVersion(30, new int[] {6, 26, 52, 78, 104, 130},
                           new ECBlocks(30, new ECB(5, 115),
                                        new ECB(10, 116)),
                           new ECBlocks(28, new ECB(19, 47),
                                        new ECB(10, 48)),
                           new ECBlocks(30, new ECB(15, 24),
                                        new ECB(25, 25)),
                           new ECBlocks(30, new ECB(23, 15),
                                        new ECB(25, 16))),
               new QRVersion(31, new int[] {6, 30, 56, 82, 108, 134},
                           new ECBlocks(30, new ECB(13, 115),
                                        new ECB(3, 116)),
                           new ECBlocks(28, new ECB(2, 46),
                                        new ECB(29, 47)),
                           new ECBlocks(30, new ECB(42, 24),
                                        new ECB(1, 25)),
                           new ECBlocks(30, new ECB(23, 15),
                                        new ECB(28, 16))),
               new QRVersion(32, new int[] {6, 34, 60, 86, 112, 138},
                           new ECBlocks(30, new ECB(17, 115)),
                           new ECBlocks(28, new ECB(10, 46),
                                        new ECB(23, 47)),
                           new ECBlocks(30, new ECB(10, 24),
                                        new ECB(35, 25)),
                           new ECBlocks(30, new ECB(19, 15),
                                        new ECB(35, 16))),
               new QRVersion(33, new int[] {6, 30, 58, 86, 114, 142},
                           new ECBlocks(30, new ECB(17, 115),
                                        new ECB(1, 116)),
                           new ECBlocks(28, new ECB(14, 46),
                                        new ECB(21, 47)),
                           new ECBlocks(30, new ECB(29, 24),
                                        new ECB(19, 25)),
                           new ECBlocks(30, new ECB(11, 15),
                                        new ECB(46, 16))),
               new QRVersion(34, new int[] {6, 34, 62, 90, 118, 146},
                           new ECBlocks(30, new ECB(13, 115),
                                        new ECB(6, 116)),
                           new ECBlocks(28, new ECB(14, 46),
                                        new ECB(23, 47)),
                           new ECBlocks(30, new ECB(44, 24),
                                        new ECB(7, 25)),
                           new ECBlocks(30, new ECB(59, 16),
                                        new ECB(1, 17))),
               new QRVersion(35, new int[] {6, 30, 54, 78, 102, 126, 150},
                           new ECBlocks(30, new ECB(12, 121),
                                        new ECB(7, 122)),
                           new ECBlocks(28, new ECB(12, 47),
                                        new ECB(26, 48)),
                           new ECBlocks(30, new ECB(39, 24),
                                        new ECB(14, 25)),
                           new ECBlocks(30, new ECB(22, 15),
                                        new ECB(41, 16))),
               new QRVersion(36, new int[] {6, 24, 50, 76, 102, 128, 154},
                           new ECBlocks(30, new ECB(6, 121),
                                        new ECB(14, 122)),
                           new ECBlocks(28, new ECB(6, 47),
                                        new ECB(34, 48)),
                           new ECBlocks(30, new ECB(46, 24),
                                        new ECB(10, 25)),
                           new ECBlocks(30, new ECB(2, 15),
                                        new ECB(64, 16))),
               new QRVersion(37, new int[] {6, 28, 54, 80, 106, 132, 158},
                           new ECBlocks(30, new ECB(17, 122),
                                        new ECB(4, 123)),
                           new ECBlocks(28, new ECB(29, 46),
                                        new ECB(14, 47)),
                           new ECBlocks(30, new ECB(49, 24),
                                        new ECB(10, 25)),
                           new ECBlocks(30, new ECB(24, 15),
                                        new ECB(46, 16))),
               new QRVersion(38, new int[] {6, 32, 58, 84, 110, 136, 162},
                           new ECBlocks(30, new ECB(4, 122),
                                        new ECB(18, 123)),
                           new ECBlocks(28, new ECB(13, 46),
                                        new ECB(32, 47)),
                           new ECBlocks(30, new ECB(48, 24),
                                        new ECB(14, 25)),
                           new ECBlocks(30, new ECB(42, 15),
                                        new ECB(32, 16))),
               new QRVersion(39, new int[] {6, 26, 54, 82, 110, 138, 166},
                           new ECBlocks(30, new ECB(20, 117),
                                        new ECB(4, 118)),
                           new ECBlocks(28, new ECB(40, 47),
                                        new ECB(7, 48)),
                           new ECBlocks(30, new ECB(43, 24),
                                        new ECB(22, 25)),
                           new ECBlocks(30, new ECB(10, 15),
                                        new ECB(67, 16))),
               new QRVersion(40, new int[] {6, 30, 58, 86, 114, 142, 170},
                           new ECBlocks(30, new ECB(19, 118),
                                        new ECB(6, 119)),
                           new ECBlocks(28, new ECB(18, 47),
                                        new ECB(31, 48)),
                           new ECBlocks(30, new ECB(34, 24),
                                        new ECB(34, 25)),
                           new ECBlocks(30, new ECB(20, 15),
                                        new ECB(61, 16)))
               };
       }
   }
   /// <summary>
   /// A simple, fast array of bits, represented compactly by an array of ints internally.
   /// </summary>
   /// <author>Sean Owen</author>
   public sealed class BitArray
   {
       private int[] bits;
       private int size;

       /// <summary>
       /// size of the array, number of elements
       /// </summary>
       public int Size
       {
           get
           {
               return size;
           }
       }

       /// <summary>
       /// size of the array in bytes
       /// </summary>
       public int SizeInBytes
       {
           get
           {
               return (size + 7) >> 3;
           }
       }

       /// <summary>
       /// index accessor
       /// </summary>
       /// <param name="i"></param>
       /// <returns></returns>
       public bool this[int i]
       {
           get
           {
               return (bits[i >> 5] & (1 << (i & 0x1F))) != 0;
           }
           set
           {
               if (value)
                   bits[i >> 5] |= 1 << (i & 0x1F);
           }
       }

       /// <summary>
       /// default constructor
       /// </summary>
       public BitArray()
       {
           this.size = 0;
           this.bits = new int[1];
       }

       /// <summary>
       /// initializing constructor
       /// </summary>
       /// <param name="size">desired size of the array</param>
       public BitArray(int size)
       {
           if (size < 1)
           {
               throw new ArgumentException("size must be at least 1");
           }
           this.size = size;
           this.bits = makeArray(size);
       }

       // For testing only
       private BitArray(int[] bits, int size)
       {
           this.bits = bits;
           this.size = size;
       }

       private void ensureCapacity(int size)
       {
           if (size > bits.Length << 5)
           {
               int[] newBits = makeArray(size);
               System.Array.Copy(bits, 0, newBits, 0, bits.Length);
               bits = newBits;
           }
       }

       /// <summary>
       /// Flips bit i.
       /// </summary>
       /// <param name="i">bit to set
       /// </param>
       public void flip(int i)
       {
           bits[i >> 5] ^= 1 << (i & 0x1F);
       }

       private static int numberOfTrailingZeros(int num)
       {
           var index = (-num & num) % 37;
           if (index < 0)
               index *= -1;
           return _lookup[index];
       }

       private static readonly int[] _lookup =
         {
            32, 0, 1, 26, 2, 23, 27, 0, 3, 16, 24, 30, 28, 11, 0, 13, 4, 7, 17,
            0, 25, 22, 31, 15, 29, 10, 12, 6, 0, 21, 14, 9, 5, 20, 8, 19, 18
         };

       /// <summary>
       /// Gets the next set.
       /// </summary>
       /// <param name="from">first bit to check</param>
       /// <returns>index of first bit that is set, starting from the given index, or size if none are set
       /// at or beyond this given index</returns>
       public int getNextSet(int from)
       {
           if (from >= size)
           {
               return size;
           }
           int bitsOffset = from >> 5;
           int currentBits = bits[bitsOffset];
           // mask off lesser bits first
           currentBits &= ~((1 << (from & 0x1F)) - 1);
           while (currentBits == 0)
           {
               if (++bitsOffset == bits.Length)
               {
                   return size;
               }
               currentBits = bits[bitsOffset];
           }
           int result = (bitsOffset << 5) + numberOfTrailingZeros(currentBits);
           return result > size ? size : result;
       }

       /// <summary>
       /// see getNextSet(int)
       /// </summary>
       /// <param name="from">index to start looking for unset bit</param>
       /// <returns>index of next unset bit, or <see cref="Size"/> if none are unset until the end</returns>
       public int getNextUnset(int from)
       {
           if (from >= size)
           {
               return size;
           }
           int bitsOffset = from >> 5;
           int currentBits = ~bits[bitsOffset];
           // mask off lesser bits first
           currentBits &= ~((1 << (from & 0x1F)) - 1);
           while (currentBits == 0)
           {
               if (++bitsOffset == bits.Length)
               {
                   return size;
               }
               currentBits = ~bits[bitsOffset];
           }
           int result = (bitsOffset << 5) + numberOfTrailingZeros(currentBits);
           return result > size ? size : result;
       }

       /// <summary> Sets a block of 32 bits, starting at bit i.
       /// 
       /// </summary>
       /// <param name="i">first bit to set
       /// </param>
       /// <param name="newBits">the new value of the next 32 bits. Note again that the least-significant bit
       /// corresponds to bit i, the next-least-significant to i+1, and so on.
       /// </param>
       public void setBulk(int i, int newBits)
       {
           bits[i >> 5] = newBits;
       }

       /// <summary>
       /// Sets a range of bits.
       /// </summary>
       /// <param name="start">start of range, inclusive.</param>
       /// <param name="end">end of range, exclusive</param>
       public void setRange(int start, int end)
       {
           if (end < start || start < 0 || end > size)
           {
               throw new ArgumentException();
           }
           if (end == start)
           {
               return;
           }
           end--; // will be easier to treat this as the last actually set bit -- inclusive
           int firstInt = start >> 5;
           int lastInt = end >> 5;
           for (int i = firstInt; i <= lastInt; i++)
           {
               int firstBit = i > firstInt ? 0 : start & 0x1F;
               int lastBit = i < lastInt ? 31 : end & 0x1F;
               // Ones from firstBit to lastBit, inclusive
               int mask = (2 << lastBit) - (1 << firstBit);
               bits[i] |= mask;
           }
       }

       /// <summary> Clears all bits (sets to false).</summary>
       public void clear()
       {
           int max = bits.Length;
           for (int i = 0; i < max; i++)
           {
               bits[i] = 0;
           }
       }

       /// <summary> Efficient method to check if a range of bits is set, or not set.
       /// 
       /// </summary>
       /// <param name="start">start of range, inclusive.
       /// </param>
       /// <param name="end">end of range, exclusive
       /// </param>
       /// <param name="value">if true, checks that bits in range are set, otherwise checks that they are not set
       /// </param>
       /// <returns> true iff all bits are set or not set in range, according to value argument</returns>
       /// <throws><exception cref="ArgumentException" /> if end is less than start or the range is not contained in the array</throws>
       public bool isRange(int start, int end, bool value)
       {
           if (end < start || start < 0 || end > size)
           {
               throw new ArgumentException();
           }
           if (end == start)
           {
               return true; // empty range matches
           }
           end--; // will be easier to treat this as the last actually set bit -- inclusive    
           int firstInt = start >> 5;
           int lastInt = end >> 5;
           for (int i = firstInt; i <= lastInt; i++)
           {
               int firstBit = i > firstInt ? 0 : start & 0x1F;
               int lastBit = i < lastInt ? 31 : end & 0x1F;
               // Ones from firstBit to lastBit, inclusive
               int mask = (2 << lastBit) - (1 << firstBit);

               // Return false if we're looking for 1s and the masked bits[i] isn't all 1s (that is,
               // equals the mask, or we're looking for 0s and the masked portion is not all 0s
               if ((bits[i] & mask) != (value ? mask : 0))
               {
                   return false;
               }
           }
           return true;
       }

       /// <summary>
       /// Appends the bit.
       /// </summary>
       /// <param name="bit">The bit.</param>
       public void appendBit(bool bit)
       {
           ensureCapacity(size + 1);
           if (bit)
           {
               bits[size >> 5] |= 1 << (size & 0x1F);
           }
           size++;
       }

       /// <returns> underlying array of ints. The first element holds the first 32 bits, and the least
       /// significant bit is bit 0.
       /// </returns>
       public int[] Array
       {
           get { return bits; }
       }

       /// <summary>
       /// Appends the least-significant bits, from value, in order from most-significant to
       /// least-significant. For example, appending 6 bits from 0x000001E will append the bits
       /// 0, 1, 1, 1, 1, 0 in that order.
       /// </summary>
       /// <param name="value"><see cref="int"/> containing bits to append</param>
       /// <param name="numBits">bits from value to append</param>
       public void appendBits(int value, int numBits)
       {
           if (numBits < 0 || numBits > 32)
           {
               throw new ArgumentException("Num bits must be between 0 and 32");
           }
           ensureCapacity(size + numBits);
           for (int numBitsLeft = numBits; numBitsLeft > 0; numBitsLeft--)
           {
               appendBit(((value >> (numBitsLeft - 1)) & 0x01) == 1);
           }
       }

       /// <summary>
       /// adds the array to the end
       /// </summary>
       /// <param name="other"></param>
       public void appendBitArray(BitArray other)
       {
           int otherSize = other.size;
           ensureCapacity(size + otherSize);
           for (int i = 0; i < otherSize; i++)
           {
               appendBit(other[i]);
           }
       }

       /// <summary>
       /// XOR operation
       /// </summary>
       /// <param name="other"></param>
       public void xor(BitArray other)
       {
           if (size != other.size)
           {
               throw new ArgumentException("Sizes don't match");
           }
           for (int i = 0; i < bits.Length; i++)
           {
               // The last int could be incomplete (i.e. not have 32 bits in
               // it) but there is no problem since 0 XOR 0 == 0.
               bits[i] ^= other.bits[i];
           }
       }

       /// <summary>
       /// converts to bytes.
       /// </summary>
       /// <param name="bitOffset">first bit to start writing</param>
       /// <param name="array">array to write into. Bytes are written most-significant byte first. This is the opposite
       /// of the internal representation, which is exposed by BitArray</param>
       /// <param name="offset">position in array to start writing</param>
       /// <param name="numBytes">how many bytes to write</param>
       public void toBytes(int bitOffset, byte[] array, int offset, int numBytes)
       {
           for (int i = 0; i < numBytes; i++)
           {
               int theByte = 0;
               for (int j = 0; j < 8; j++)
               {
                   if (this[bitOffset])
                   {
                       theByte |= 1 << (7 - j);
                   }
                   bitOffset++;
               }
               array[offset + i] = (byte)theByte;
           }
       }

       /// <summary> Reverses all bits in the array.</summary>
       public void reverse()
       {
           var newBits = new int[bits.Length];
           // reverse all int's first
           var len = ((size - 1) >> 5);
           var oldBitsLen = len + 1;
           for (var i = 0; i < oldBitsLen; i++)
           {
               var x = (long)bits[i];
               x = ((x >> 1) & 0x55555555u) | ((x & 0x55555555u) << 1);
               x = ((x >> 2) & 0x33333333u) | ((x & 0x33333333u) << 2);
               x = ((x >> 4) & 0x0f0f0f0fu) | ((x & 0x0f0f0f0fu) << 4);
               x = ((x >> 8) & 0x00ff00ffu) | ((x & 0x00ff00ffu) << 8);
               x = ((x >> 16) & 0x0000ffffu) | ((x & 0x0000ffffu) << 16);
               newBits[len - i] = (int)x;
           }
           // now correct the int's if the bit size isn't a multiple of 32
           if (size != oldBitsLen * 32)
           {
               var leftOffset = oldBitsLen * 32 - size;
               var currentInt = ((int)((uint)newBits[0] >> leftOffset)); // (newBits[0] >>> leftOffset);
               for (var i = 1; i < oldBitsLen; i++)
               {
                   var nextInt = newBits[i];
                   currentInt |= nextInt << (32 - leftOffset);
                   newBits[i - 1] = currentInt;
                   currentInt = ((int)((uint)nextInt >> leftOffset)); // (nextInt >>> leftOffset);
               }
               newBits[oldBitsLen - 1] = currentInt;
           }
           bits = newBits;
       }

       private static int[] makeArray(int size)
       {
           return new int[(size + 31) >> 5];
       }

       /// <summary>
       /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
       /// </summary>
       /// <param name="o">The <see cref="System.Object"/> to compare with this instance.</param>
       /// <returns>
       ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
       /// </returns>
       public override bool Equals(Object o)
       {
           var other = o as BitArray;
           if (other == null)
               return false;
           if (size != other.size)
               return false;
           for (var index = 0; index < bits.Length; index++)
           {
               if (bits[index] != other.bits[index])
                   return false;
           }
           return true;
       }

       /// <summary>
       /// Returns a hash code for this instance.
       /// </summary>
       /// <returns>
       /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
       /// </returns>
       public override int GetHashCode()
       {
           var hash = size;
           foreach (var bit in bits)
           {
               hash = 31 * hash + bit.GetHashCode();
           }
           return hash;
       }

       /// <summary>
       /// Returns a <see cref="System.String"/> that represents this instance.
       /// </summary>
       /// <returns>
       /// A <see cref="System.String"/> that represents this instance.
       /// </returns>
       public override String ToString()
       {
           var result = new System.Text.StringBuilder(size);
           for (int i = 0; i < size; i++)
           {
               if ((i & 0x07) == 0)
               {
                   result.Append(' ');
               }
               result.Append(this[i] ? 'X' : '.');
           }
           return result.ToString();
       }

       /// <summary>
       /// Erstellt ein neues Objekt, das eine Kopie der aktuellen Instanz darstellt.
       /// </summary>
       /// <returns>
       /// Ein neues Objekt, das eine Kopie dieser Instanz darstellt.
       /// </returns>
       public object Clone()
       {
           return new BitArray((int[])bits.Clone(), size);
       }
   }

}
