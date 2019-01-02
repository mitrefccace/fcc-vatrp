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

namespace VATRP.LinphoneWrapper.Enums
{
    public enum MSVideoSize
    {
        MS_VIDEO_SIZE_UNKNOWN_W = 0,
        MS_VIDEO_SIZE_UNKNOWN_H = 0,

        MS_VIDEO_SIZE_SQCIF_W = 128,
        MS_VIDEO_SIZE_SQCIF_H = 96,

        MS_VIDEO_SIZE_WQCIF_W = 256,
        MS_VIDEO_SIZE_WQCIF_H = 144,

        MS_VIDEO_SIZE_QCIF_W = 176,
        MS_VIDEO_SIZE_QCIF_H = 144,

        MS_VIDEO_SIZE_CIF_W = 352,
        MS_VIDEO_SIZE_CIF_H = 288,

        MS_VIDEO_SIZE_CVD_W = 352,
        MS_VIDEO_SIZE_CVD_H = 480,

        MS_VIDEO_SIZE_ICIF_W = 352,
        MS_VIDEO_SIZE_ICIF_H = 576,

        MS_VIDEO_SIZE_4CIF_W = 704,
        MS_VIDEO_SIZE_4CIF_H = 576,

        MS_VIDEO_SIZE_W4CIF_W = 1024,
        MS_VIDEO_SIZE_W4CIF_H = 576,

        MS_VIDEO_SIZE_QQVGA_W = 160,
        MS_VIDEO_SIZE_QQVGA_H = 120,

        MS_VIDEO_SIZE_HQVGA_W = 160,
        MS_VIDEO_SIZE_HQVGA_H = 240,

        MS_VIDEO_SIZE_QVGA_W = 320,
        MS_VIDEO_SIZE_QVGA_H = 240,

        MS_VIDEO_SIZE_HVGA_W = 320,
        MS_VIDEO_SIZE_HVGA_H = 480,

        MS_VIDEO_SIZE_VGA_W = 640,
        MS_VIDEO_SIZE_VGA_H = 480,

        MS_VIDEO_SIZE_SVGA_W = 800,
        MS_VIDEO_SIZE_SVGA_H = 600,

        MS_VIDEO_SIZE_NS1_W = 324,
        MS_VIDEO_SIZE_NS1_H = 248,

        MS_VIDEO_SIZE_QSIF_W = 176,
        MS_VIDEO_SIZE_QSIF_H = 120,

        MS_VIDEO_SIZE_SIF_W = 352,
        MS_VIDEO_SIZE_SIF_H = 240,

        MS_VIDEO_SIZE_IOS_MEDIUM_W = 480,
        MS_VIDEO_SIZE_IOS_MEDIUM_H = 360,

        MS_VIDEO_SIZE_ISIF_W = 352,
        MS_VIDEO_SIZE_ISIF_H = 480,

        MS_VIDEO_SIZE_4SIF_W = 704,
        MS_VIDEO_SIZE_4SIF_H = 480,

        MS_VIDEO_SIZE_288P_W = 512,
        MS_VIDEO_SIZE_288P_H = 288,

        MS_VIDEO_SIZE_432P_W = 768,
        MS_VIDEO_SIZE_432P_H = 432,

        MS_VIDEO_SIZE_448P_W = 768,
        MS_VIDEO_SIZE_448P_H = 448,

        MS_VIDEO_SIZE_480P_W = 848,
        MS_VIDEO_SIZE_480P_H = 480,

        MS_VIDEO_SIZE_576P_W = 1024,
        MS_VIDEO_SIZE_576P_H = 576,

        MS_VIDEO_SIZE_720P_W = 1280,
        MS_VIDEO_SIZE_720P_H = 720,

        MS_VIDEO_SIZE_1080P_W = 1920,
        MS_VIDEO_SIZE_1080P_H = 1080,

        MS_VIDEO_SIZE_SDTV_W = 768,
        MS_VIDEO_SIZE_SDTV_H = 576,

        MS_VIDEO_SIZE_HDTVP_W = 1920,
        MS_VIDEO_SIZE_HDTVP_H = 1200,

        MS_VIDEO_SIZE_XGA_W = 1024,
        MS_VIDEO_SIZE_XGA_H = 768,

        MS_VIDEO_SIZE_WXGA_W = 1080,
        MS_VIDEO_SIZE_WXGA_H = 768,

        MS_VIDEO_SIZE_SXGA_MINUS_W = 1280,
        MS_VIDEO_SIZE_SXGA_MINUS_H = 960,

        MS_VIDEO_SIZE_UXGA_W = 1600,
        MS_VIDEO_SIZE_UXGA_H = 1200,
    }
}