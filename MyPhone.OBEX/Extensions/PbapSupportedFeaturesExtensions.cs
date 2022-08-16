using GoodTimeStudio.MyPhone.OBEX.Headers;
using GoodTimeStudio.MyPhone.OBEX.Pbap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.OBEX.Extensions
{
    public static class PbapSupportedFeaturesExtensions
    {
        public static AppParameter ToAppParameter(this PbapSupportedFeatures supportedFeatures)
        {
            return new AppParameter((byte)PbapAppParamTagId.PbapSupportedFeatures, (int)supportedFeatures);
        }

        public static bool Supports(this PbapSupportedFeatures supportedFeatures, PbapSupportedFeatures features)
        {
            return (supportedFeatures & features) == features;
        }
    }
}
