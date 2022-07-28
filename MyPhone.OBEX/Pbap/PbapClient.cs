using MixERP.Net.VCards;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace MyPhone.OBEX
{
    /// <summary>
    /// Phone Book Access Profile client
    /// </summary>
    public class PbapClient : ObexClient
    {
        public PbapClient(IInputStream inputStream, IOutputStream outputStream) : base(inputStream, outputStream)
        {
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
        public async Task<string> PullPhoneBook(string phoneBookObjectPath)
        {
            ObexPacket request = new ObexPacket(new ObexOpcode(ObexOperation.Get, true));
            request.Headers[HeaderId.Name] = new UnicodeStringValueHeader(HeaderId.Name, phoneBookObjectPath);
            request.Headers[HeaderId.Type] = new AsciiStringValueHeader(HeaderId.Type, "x-bt/phonebook");
            request.Headers[HeaderId.ApplicationParameters] = new AppParamHeader();
            
            ObexPacket response = await RunObexRequestAsync(request);

            Console.WriteLine(((BodyHeader)response.Headers[HeaderId.Body]).Value);
            return ((BodyHeader)response.Headers[HeaderId.Body]).Value!;
        }

        public async Task<IEnumerable<VCard>> GetAllContacts()
        {
            string str = await PullPhoneBook("telecom/pb.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetCombinedCallHistory()
        {
            string str = await PullPhoneBook("telecom/cch.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetIncomingCallHistory()
        {
            string str = await PullPhoneBook("telecom/ich.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetOutgoingCallHistory()
        {
            string str = await PullPhoneBook("telecom/och.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetMissedCallHistory()
        {
            string str = await PullPhoneBook("telecom/mch.vcf");
            return Deserializer.GetVCards(str);
        }

        public async Task<IEnumerable<VCard>> GetSpeedDail()
        {
            string str = await PullPhoneBook("telecom/spd.vcf");
            return Deserializer.GetVCards(str);
        }

    }
}
