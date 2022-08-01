using System;
using System.Collections.Generic;
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
            using (DataWriter writer = new DataWriter())
            {
                foreach (AppParameter appParam in AppParameters)
                {
                    writer.WriteByte(appParam.TagId);
                    writer.WriteByte(appParam.ContentLength);
                    writer.WriteBuffer(appParam.Buffer);
                }
                return new ObexHeader(HeaderId.ApplicationParameters, writer.DetachBuffer());
            }
        }
    }
}
