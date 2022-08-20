using GoodTimeStudio.MyPhone.OBEX.Headers;
using MixERP.Net.VCards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace GoodTimeStudio.MyPhone.OBEX.Pbap
{
    /// <summary>
    /// Phone Book Access Profile client
    /// </summary>
    public class PbapClient : ObexClient
    {
        public PbapSupportedFeatures SupportedFeatures { get; }

        public Version ProfileVersion { get; }

        /// <remarks>
        /// Not null after connected.
        /// </remarks>
        private ObexHeader? _connectionIdHeader;

        public PbapClient(IInputStream inputStream, IOutputStream outputStream, PbapSupportedFeatures supportedFeatures, Version profileVersion) : base(inputStream, outputStream)
        {
            SupportedFeatures = supportedFeatures;
            ProfileVersion = profileVersion;
        }

        protected override void OnConnected(ObexPacket connectionResponse)
        {
            _connectionIdHeader = connectionResponse.GetHeader(HeaderId.ConnectionId);
        }

        /// <summary>
        /// Retrieves an phone book object from the object exchange server.
        /// </summary>
        /// <param name="phoneBookObjectPath">
        /// Absolute path in the virtual folders architecture of the PSE, 
        /// appended with the name of the file representation of one of the Phone Book Objects.
        /// Example: telecom/pb.vcf or SIM1/telecom/pb.vcf for the main phone book objects
        /// </param>
        /// <returns>phone book object string</returns>
        public async Task<string> PullPhoneBookAsync(string phoneBookObjectPath)
        {
            ObexPacket request = new ObexPacket(new ObexOpcode(ObexOperation.Get, true),
                _connectionIdHeader!,
                new ObexHeader(HeaderId.Name, phoneBookObjectPath, true, Encoding.BigEndianUnicode),
                new ObexHeader(HeaderId.Type, "x-bt/phonebook", true, Encoding.UTF8),
                new AppParameterHeaderBuilder(
                    new AppParameter((byte)PbapAppParamTagId.Format, (byte)1)).Build()
                );

            ObexPacket response = await RunObexRequestAsync(request);
            return response.GetBodyContentAsUtf8String(true);
        }

        public async Task<IEnumerable<VCard>> GetAllContactsAsync()
        {
            string str = await PullPhoneBookAsync("telecom/pb.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetCombinedCallHistoryAsync()
        {
            string str = await PullPhoneBookAsync("telecom/cch.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetIncomingCallHistoryAsync()
        {
            string str = await PullPhoneBookAsync("telecom/ich.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetOutgoingCallHistoryAsync()
        {
            string str = await PullPhoneBookAsync("telecom/och.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetMissedCallHistoryAsync()
        {
            string str = await PullPhoneBookAsync("telecom/mch.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetSpeedDailAsync()
        {
            string str = await PullPhoneBookAsync("telecom/spd.vcf");
            return Deserializer.GetVCards(str);
        }

    }
}
