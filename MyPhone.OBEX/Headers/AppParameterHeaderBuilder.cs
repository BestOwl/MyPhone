using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Headers
{
    public class AppParameterHeaderBuilder
    {

        public List<AppParameter> AppParameters { get; }

        public AppParameterHeaderBuilder(params AppParameter[] appParameters)
        {
            AppParameters = new List<AppParameter>();
            AppParameters.AddRange(appParameters);
        }

        public ObexHeader Build()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream))
            {
                foreach (AppParameter appParam in AppParameters)
                {
                    writer.Write(appParam.TagId);
                    writer.Write(BinaryPrimitives.ReverseEndianness(appParam.ContentLength));
                    writer.Write(appParam.Buffer);
                }
                writer.Flush();
                return new ObexHeader(HeaderId.ApplicationParameters, memoryStream.ToArray());
            }
        }
    }
}
