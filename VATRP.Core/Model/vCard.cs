#region copyright
/**
 * Copyright © The MITRE Corporation.
 *
 * This program is licensed under the terms of the GNU General Public License Version 2, as published by the Free Software Foundation. This program is distributed WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  Please see the
 * GNU General Public License for more details.
 
 * This program or code contains content developed by The MITRE Corporation. If this program is used in a deployment or embedded within another project, MITRE requests that you send an email to opensource@mitre.org to let us know where and how this program is being used.
 
 * NOTICE
 * This (software/technical data) was produced for the U. S. Government under Contract Number HHSM-500-2012-00008I, and is subject to Federal Acquisition Regulation Clause 52.227-14, Rights in Data-General. No other use other than that granted to the U. S. Government, or to those acting on behalf of the U. S. Government under that Clause is authorized without the express written permission of The MITRE Corporation. For further information, please contact The MITRE Corporation, Contracts Management Office, 7515 Colshire Drive, McLean, VA  22102-7539, (703) 983-6000.
 */
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using VATRP.Core.Extensions;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;


namespace VATRP.Core.Model
{
    //There are times these classes contain objects of other classes 
    //and that is done primarily to correctly format the xml output when 
    //exporting contacts

    [Serializable]
    [XmlType("vcard")]
    public class vCard
    {
        #region Properties

        [XmlElement(ElementName = "text")]
        public string FormattedName { get; set; }

        [XmlElement(ElementName = "n")]
        public vCardContent content { get; set; }

        [XmlElement(ElementName = "tel")]
        public Telephone Telephone { get; set; }

        [XmlIgnore]
        public string Surname {
            get{
                return this.content.Surname;
            }
            set{
                this.content.Surname = value;
            }
        }

        [XmlIgnore]
        public string GivenName
        {
            get
            {
                return this.content.GivenName;
            }
            set
            {
                this.content.GivenName = value;
            }
        }

        [XmlIgnore]
        public string MiddleName
        {
            get
            {
                return this.content.MiddleName;
            }
            set
            {
                this.content.MiddleName = value;
            }
        }

        [XmlIgnore]
        public string Prefix
        {
            get
            {
                return this.content.Prefix;
            }
            set
            {
                this.content.Prefix = value;
            }
        }

        [XmlIgnore]
        public string Suffix
        {
            get
            {
                return this.content.Suffix;
            }
            set
            {
                this.content.Suffix = value;
            }
        }

        [XmlIgnore]
        public string Title { get; set; }

        [XmlIgnore]
        public string ProdId { get; set; }

        [XmlIgnore]
        public DateTime Rev { get; set; }
        #endregion

        public vCard()
        {
            content = new vCardContent();
            Telephone = new Telephone();

            FormattedName = string.Empty;
            Surname = string.Empty;
            GivenName = string.Empty;
            MiddleName = string.Empty;
            Prefix = string.Empty;
            Suffix = string.Empty;
            Title = string.Empty;
            Rev = DateTime.Now;
            ProdId = string.Empty;

            
            content.Surname = Surname;
            content.GivenName = GivenName;
            content.MiddleName = MiddleName;
            content.Prefix = Prefix;
            content.Suffix = Suffix;
        }
    }

    public class vCardContent
    {
        [XmlElement(ElementName = "surname")]
        public string Surname { get; set; }

        [XmlElement(ElementName = "given")]
        public string GivenName { get; set; }

        [XmlElement(ElementName = "middlename")]
        public string MiddleName { get; set; }

        [XmlElement(ElementName = "prefix")]
        public string Prefix { get; set; }

        [XmlElement(ElementName = "suffix")]
        public string Suffix { get; set; }

    }

    public class Telephone
    {
        [XmlElement(ElementName = "uri")]
        public string Uri { get; set; }
    }

    [XmlType("vcards")]
    public class ContactList
    {
        public List<vCard> VCards { get; set; }
        public XmlSerializerNamespaces Namespaces { get; set; }
    }

    public class vCardReader
    {
        #region Members

        private static readonly ILog LOG = LogManager.GetLogger(typeof (vCardReader));

        public List<vCard> vCards = new List<vCard>();
        #endregion

        public vCardReader(TextReader stream)
        {
            var lines = new StringBuilder();
            try
            {
                if (stream != null)
                    lines.Append(stream.ReadToEnd());
            }
            catch (Exception ex)
            {
                LOG.Error("Failed to read stream");
                return;
            }

            ParseAll(lines);
        }

        public vCardReader(string filePath)
        {
            if (!File.Exists(filePath))
            {
                LOG.Error("File does not exists. " + filePath);
                return;
            }

            var lines = new StringBuilder();
            try
            {
                lines.Append(File.ReadAllText(filePath));

            }
            catch (Exception ex)
            {
                LOG.Error("Failed to read text file: " + filePath);
                return;
            }

            ParseAll(lines);
        }

        public vCardReader(StringBuilder lines)
        {
            ParseAll(lines);
        }

        private void ParseAll(StringBuilder lines)
        {
            int posStart = lines.ToString().IndexOf("BEGIN:VCARD");
            int posPosEnd = lines.ToString().IndexOf("END:VCARD");
            while (posStart != -1 && posPosEnd != -1 && posPosEnd > posStart)
            {
                int endOfVcard = posPosEnd + "END:VCARD".Length + 1;
                var vCardText = lines.ToString(posStart, endOfVcard);
                Parse(vCardText);
                lines.Remove(0, endOfVcard+1);
                posStart = lines.ToString().IndexOf("BEGIN:VCARD");
                posPosEnd = lines.ToString().IndexOf("END:VCARD");
            }
        }

        private void Parse(string lines)
        {
            vCard card = new vCard();
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Multiline |
                                   RegexOptions.IgnorePatternWhitespace;

            Regex regex;
            Match m;
            MatchCollection mc;

            regex = new Regex(@"(?<strElement>(FN))   (:(?<strFN>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.FormattedName = m.Groups["strFN"].Value;

            // N
            regex =
                new Regex(
                    @"(\n(?<strElement>(N)))   (:(?<strSurname>([^;]*))) (;(?<strGivenName>([^;]*)))  (;(?<strMidName>([^;]*))) (;(?<strPrefix>([^;]*))) (;(?<strSuffix>[^\n\r]*))",
                    options);
            m = regex.Match(lines);
            if (m.Success)
            {
                card.Surname = m.Groups["strSurname"].Value;
                card.GivenName = m.Groups["strGivenName"].Value;
                card.MiddleName = m.Groups["strMidName"].Value;
                card.Prefix = m.Groups["strPrefix"].Value;
                card.Suffix = m.Groups["strSuffix"].Value;
            }

            // TITLE
            regex = new Regex(@"(?<strElement>(TITLE))   (:(?<strTITLE>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.Title = m.Groups["strTITLE"].Value;

            // PRODID
            regex = new Regex(@"(?<strElement>(PRODID))   (:(?<strPRODID>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
                card.ProdId = m.Groups["strPRODID"].Value;

            // REV
            regex = new Regex(@"(?<strElement>(REV))   (:(?<strREV>[^\n\r]*))", options);
            m = regex.Match(lines);
            if (m.Success)
            {
                string[] expectedFormats = { "yyyyMMddHHmmss", "yyyy-MM-ddTHHmmssZ", "yyyyMMddTHHmmssZ" };
                card.Rev = DateTime.ParseExact(m.Groups["strREV"].Value, expectedFormats, null, System.Globalization.DateTimeStyles.AllowWhiteSpaces);
            }

            if (card.Title.NotBlank() && card.FormattedName.NotBlank())
                vCards.Add(card);
        }
    }

    public class vCardWriter
    {
        private static readonly ILog LOG = LogManager.GetLogger(typeof(vCardWriter));
        public vCardWriter()
        {
            
        }

        public void WriteCards(string filePath, List<vCard> vCards)
        {
            var output = new StringBuilder();
            if (vCards != null)
            {
                foreach (var card in vCards)
                {
                    Write(ref output, card);
                }
            }

            try
            {
                var stream = File.CreateText(filePath);
                stream.Write(output.ToString());
                stream.Close();
            }
            catch (Exception ex)
            {
                LOG.Error("Failed to write vCard file: " + filePath);
            }
        }

        // Added 2/21/2017 fjr
        public void WriteCardsAsXML(string filePath, List<vCard> vCards)
        {
            var output = new StringBuilder();

            if (vCards != null)
            {
                output.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine);
                output.Append("<vcards xmlns=\"urn:ietf:params:xml:ns:vcard-4.0\">" + Environment.NewLine);

                foreach (var card in vCards)
                {
                    WriteXML(ref output, card);

                    output.Append("<x-file>" + Environment.NewLine);
                    output.Append("<parameters>" + Environment.NewLine);
                    output.Append("<mediatype><text>image/jpeg</text></mediatype>" + Environment.NewLine);
                    output.Append("</parameters>" + Environment.NewLine);
                    output.Append("<unknown>alien.jpg</unknown>" + Environment.NewLine);
                    output.Append("</x-file>" + Environment.NewLine);
                    output.Append("<a xmlns=\"http://www.w3.org/1999/xhtml\" href=\"http://www.example.com\">web page</a>" + Environment.NewLine);
                    output.Append("<email>" + Environment.NewLine);
                    output.Append("<parameters>" + Environment.NewLine);
                    output.Append("<type>" + Environment.NewLine);
                    output.Append("<text>work</text>" + Environment.NewLine);
                    output.Append("</type>" + Environment.NewLine);
                    output.Append("</parameters>" + Environment.NewLine);
                    if (card.Title.NotBlank())
                    {
                        output.Append("<text>" + card.Title + "</text>" + Environment.NewLine);
                    }
                    else
                    {
                         output.Append("<text/>");
                    }
                    output.Append("</email>" + Environment.NewLine);

                    output.Append("</vcard>" + Environment.NewLine);
                }
                //  Close vCards tag
                output.Append("</vcards>" + Environment.NewLine);
            }

            try
            {
                var stream = File.CreateText(filePath);
                stream.Write(output.ToString());
                stream.Close();
            }
            catch (Exception ex)
            {
                LOG.Error("Failed to write vCard file as XML: " + filePath);
            }
        }

        private void Write(ref StringBuilder output, vCard card)
        {
            if (card == null)
                return;

            output.Append("BEGIN:VCARD" + Environment.NewLine + "VERSION:3.0" + Environment.NewLine);
            output.Append("N:" + card.Surname + ";" + card.GivenName + ";" + card.MiddleName + ";" + 
                card.Prefix + ";" + card.Suffix + Environment.NewLine);
            output.Append("FN:" + card.FormattedName + Environment.NewLine);
            if (card.Title.NotBlank())
                output.Append("TITLE:" + card.Title + Environment.NewLine);
            output.Append("END:VCARD" + Environment.NewLine);
        }

        private void WriteXML(ref StringBuilder output, vCard card)
        {
            if (card == null)
                return;

            output.Append("<vcard>" + Environment.NewLine);
            output.Append("\t" + "<fn><text>" + card.FormattedName + "</text></fn>" + Environment.NewLine);
            output.Append("\t" + "<n>" + Environment.NewLine);
            output.Append("\t" + "\t" + "<surname>" + card.Surname + "</surname>" + Environment.NewLine);
            output.Append("\t" + "\t" + "<given>" + card.GivenName + "</given>" + Environment.NewLine);
            output.Append("\t" + "\t" + "<additional/>" + Environment.NewLine);
            output.Append("\t" + "\t" + "<prefix>" + card.Prefix + "</prefix>" + Environment.NewLine);
            //output.Append("\t" + "\t" + "<suffix>" + card.Suffix + "</suffix>" + Environment.NewLine);
            output.Append("\t" + "\t" + "<suffix/>" + Environment.NewLine);
            output.Append("\t" + "</n>" + Environment.NewLine);


            // if (card.Title.NotBlank())
            //     output.Append("TITLE:" + card.Title + Environment.NewLine);
           

        }
    }
}