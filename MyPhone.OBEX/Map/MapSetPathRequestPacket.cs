using GoodTimeStudio.MyPhone.OBEX.Headers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Map
{
    /// <summary>
    /// The request packet for SetPath function (this function is used to set the "current folder")
    /// </summary>
    public class MapSetPathRequestPacket : ObexPacket
    {
        public SetPathMode SetPathMode { get; private set; }
        public string TargetFolderName { get; }

        /// <summary>
        /// Construct a request packet to SetPath (set the "current folder")
        /// </summary>
        /// <param name="mode">Indicates the direction of folder traversal</param>
        /// <param name="folderName">
        /// Name of the fodler.
        /// 
        /// If the <paramref name="mode"/> is <see cref="SetPathMode.BackToRoot"/>, the folderName must be empty.
        /// 
        /// If the <paramref name="mode"/> is <see cref="SetPathMode.EnterFolder"/>, 
        /// the folderName should be the child folder name and it must NOT be empty.
        /// 
        /// If the <paramref name="mode"/> is <see cref="SetPathMode.BackToParent"/>, the folderName is optional (either empty or peer folder name).
        /// If the folderName is empty, go back to the parent folder. 
        /// If the folderName is a peer folder name (child of the parent folder), go back to the parent folder first then enter the child folder.
        /// Equivalent to "cd ../folderName"
        /// </param>
        /// 
        public MapSetPathRequestPacket(SetPathMode mode, string folderName = "") : base(new ObexOpcode(ObexOperation.SetPath, true))
        {
            SetPathMode = mode;

            switch (SetPathMode)
            {
                case SetPathMode.BackToRoot:
                    if (folderName != "")
                    {
                        throw new ArgumentException($"The folderName must be empty in this mode {mode}", nameof(folderName));
                    }
                    break;
                case SetPathMode.EnterFolder:
                    if (string.IsNullOrEmpty(folderName))
                    {
                        throw new ArgumentException($"The folderName must NOT be empty in this mode {mode}", nameof(folderName));
                    }
                    break;
            }

            TargetFolderName = folderName;
            Headers[HeaderId.Name] = new ObexHeader(HeaderId.Name, folderName, true, Encoding.BigEndianUnicode);
        }

        protected override void WriteExtraField(DataWriter writer)
        {
            switch (SetPathMode)
            {
                case SetPathMode.BackToRoot:
                case SetPathMode.EnterFolder:
                    writer.WriteByte(0x40); // 0100 0000
                    break;
                case SetPathMode.BackToParent:
                    writer.WriteByte(0xC0); // 1100 0000
                    break;
            }
            writer.WriteByte(0); // Reserved Constants field
        }

        protected async override Task<uint> ReadExtraField(DataReader reader)
        {
            await reader.LoadAsync(2);
            byte flag = reader.ReadByte();
            flag = (byte)(flag & 0x03);
            if (flag == 0x10)
            {
                if (TargetFolderName.Length == 0)
                {
                    SetPathMode = SetPathMode.BackToRoot;
                }
                else
                {
                    SetPathMode = SetPathMode.EnterFolder;
                }
            }
            else if (flag == 0x11)
            {
                SetPathMode = SetPathMode.BackToParent;
            }
            else // 0x00
            {
                throw new ObexException("Invalid SetPath packet");
            }

            return 2;
        }
    }

    public enum SetPathMode
    {
        /// <summary>
        /// Go back to the root folder (equivalent to "cd /")
        /// </summary>
        BackToRoot,

        /// <summary>
        /// Navigate to the parent folder (equivalent to "cd ../")
        /// </summary>
        BackToParent,

        /// <summary>
        /// Navigate to the child folder (equivalent to "cd folderName")
        /// </summary>
        EnterFolder
    }
}
