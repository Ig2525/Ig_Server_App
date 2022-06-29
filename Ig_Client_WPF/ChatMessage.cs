using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
//using System.Drawing.;

namespace Ig_Client_WPF
{
    public enum TypeMessage
    {
        Login,
        Logout,
        Message
    }
    internal class ChatMessage
    {
        public TypeMessage MessageType;
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
        //public byte[] Image;
        //public BitmapImage Avatar;

        //public byte[] Image
        //{
        //    get => (byte[])(new ImageConverter()).ConvertTo(image, typeof(byte[]));
        //    set
        //    {
        //        image = new BitmapImage();
        //    }
        //}

        public byte[] Serialize()
        {
            using (MemoryStream m = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(m))
                {
                    bw.Write((int)MessageType);
                    bw.Write(UserId);
                    bw.Write(UserName);
                    bw.Write(Text);
                    //bw.Write(Convert.ToString(Avatar));
                }
                return m.ToArray();
            }
        }

        public static ChatMessage Deserialize(byte[] data)
        {
            ChatMessage message = new ChatMessage();
            using (MemoryStream m = new MemoryStream(data))
            {
                using (BinaryReader br = new BinaryReader(m))
                {
                    message.MessageType = (TypeMessage)br.ReadInt32();
                    message.UserId = br.ReadString();
                    message.UserName = br.ReadString();
                    message.Text = br.ReadString();
                    //message.Avatar = (BitmapImage)br.ReadString();
                    //message.Text = br.
                }
            }
            return message;
        }

        //public byte[] imageToByteArray(System.Drawing.Image imageIn)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
        //    return ms.ToArray();
        //}

        //public Image byteArrayToImage(byte[] byteArrayIn)
        //{
        //    MemoryStream ms = new MemoryStream(byteArrayIn);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}

        //public byte[] ImageToByteArray(Image img)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    img.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
        //    return ms.ToArray();
        //}
        //public Image ByteArrayToImage(byte[] data)
        //{
        //    MemoryStream ms = new MemoryStream(data);
        //    Image returnImage = Image.FromStream(ms);
        //    return returnImage;
        //}

        //public string ImgToStr(string filename)
        //{
        //    MemoryStream Memostr = new MemoryStream();
        //    Image Img = Image.FromFile(filename);
        //    Img.Save(Memostr, Img.RawFormat);
        //    byte[] arrayimg = Memostr.ToArray();
        //    return Convert.ToBase64String(arrayimg);
        //}

        ////Преобразование строки в изображение
        //public Image StrToImg(string StrImg)
        //{
        //    byte[] arrayimg = Convert.FromBase64String(StrImg);
        //    Image imageStr = Image.FromStream(new MemoryStream(arrayimg));
        //    return imageStr;
        //}
    }
}
