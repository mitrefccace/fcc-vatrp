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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VATRP.Core.Model
{
    // will be parsed as a json resource
    // eg:
    //[
    // {"name": "ACETest Registrar", "domain": "acetest-registrar.vatrp.net", "icon": "http://cdn.vatrp.net/acetest.png", "icon2x": "http://cdn.vatrp.net/acetest2x.png" },
    // {"name": "BC1", "domain": "bc1.vatrp.net", "icon": "http://cdn.vatrp.net/belledonne.png", "icon2x": "http://cdn.vatrp.net/belledonne2x.png" },
    // {"name": "Star", "domain": "caag.vatrp.net", "icon": "http://cdn.vatrp.net/caag.png", "icon2x": "http://cdn.vatrp.net/caag2x.png" },
    // {"name": "Convo", "domain": "convo.vatrp.net", "icon": "http://cdn.vatrp.net/convo.png", "icon2x": "http://cdn.vatrp.net/convo2x.png" },
    // {"name": "Global", "domain": "global.vatrp.net", "icon": "http://cdn.vatrp.net/global.png", "icon2x": "http://cdn.vatrp.net/global2x.png" },
    // {"name": "Purple", "domain": "purple.vatrp.net", "icon": "http://cdn.vatrp.net/purple.png", "icon2x": "http://cdn.vatrp.net/purple2x.png" },
    // {"name": "Sorenson", "domain": "sorenson.vatrp.net", "icon": "http://cdn.vatrp.net/sorenson.png", "icon2x": "http://cdn.vatrp.net/sorenson2.png" },
    // {"name": "ZVRS", "domain": "zvrs.vatrp.net", "icon": "http://cdn.vatrp.net/csdvrs.png", "icon2x": "http://cdn.vatrp.net/csdvrs2x.png" }
    //]
    //public class VATRPDomain
    //{


    //    public string name { get; set; }
    //    public string domain { get; set; }
    //    public string icon { get; set; }
    //    public string icon2x { get; set; }

    //    public VATRPDomain()
    //    {
    //    }
    //}



    public class VATRPDomain
    {


        public string name { get; set; }
        public string domain { get; set; }
        public string icon { get; set; }
        public string icon2x { get; set; }

        public VATRPDomain()
        {
        }
    }


    //public class ProvidersList
    //{

    //    public List<providers> providers { get; set; }
    //}

    //public class providers
    //{

    //    public string name { get; set; }
    //    public string domain { get; set; }
    //    public string icon { get; set; }
    //    public string icon2x { get; set; }
    //}



    //public class providers
    //{
    //    public string name { get; set; }
    //    public string domain { get; set; }
    //    public string icon { get; set; }
    //    public string icon2x { get; set; }


    //}
    //public class providersList
    //{
    //    public List<providers> providers { get; set; }
    //}






}
