![](VATRP.App/Resources/about.png)

# VATRP 

### Role

The Video Access Technology Reference Platform (VATRP), formerly known as the ACE APP, is a Windows desktop application that can perform video relay services (VRS) for the community of deaf and hard of hearing individuals. VRS is a form of Telecommunications Relay Service (TRS) that enables people who are deaf or hard of hearing and use sign language, such as American Sign Language (ASL), to communicate with voice telephone users through video equipment. These services also enable communication between such individuals directly in suitable modalities, including any combination of sign language via video, real-time text (RTT), and speech.

Today, the Federal Communications Commission (FCC) utilizes the VATRP as a functional test tool for assessing and ensuring interoperability between VRS providers. In summary, the VATRP provides the following distinctive value:

* Test tool to accurately assess and test for (Relay User Equipment) RUE specification compliance among VRS provider services and devices.
* Test tool to accurately assess and test interoperability among VRS provider services and devices.
* Troubleshooting tool for the VRS providers to identify, validate, and resolve intra and inter-operability and RUE specification compliance issues.


### Development Environment Setup

Minimum Requirements:

* Windows 10.X Operating System (OS)
* Webcam
* Microphone
* Microsoft Visual C++ 2013 Redistributable
* Microsoft Visual Studio 2015 (Update 3)

When installing the Microsoft Visual Studio IDE, navigate to the “Older Versions” sections of the download page and then continue with Visual Studio Community 2015 Update 3.

Downloading the IDE may require creation of a Microsoft 360 account. During the installation process, select a custom install.

On the following page, check the box that selects all the available installation options and then de-select the C++ package. The installation may take several hours to complete.

The VATRP has been provisioned with an installer project that simplifies the process of installing the endpoint client device. Execute the following steps to install Microsoft Visual Studio:

1.	Run Visual Studio as Administrator and select the “Extensions and Updates” option from the Tools dropdown.
2.	Search 'installer' while in the online section.
3.	Download and install Microsoft Visual Studio 2015 Installer Projects.
4.	Close the running instance of Visual Studio and run the VSI_bundle.exe that was just downloaded.

### Building the Application 
Once Visual Studio has been installed and this repository has been cloned:

1.	Open the VATRP.sln file within the IDE.
2.	Open the solution folder and right-click the VATRP solution.
3.	Clean and rebuild the application before running it for the first time.

By default, the VATRP App project should already be selected as the default project to run. The navigation bar at the top of Visual Studio contains a green “Play” arrow icon that, when clicked, will build and run the application in debug mode. To run the application outside this mode, press Ctrl F5 as a short cut.

### Reference Errors
If there are any issues regarding an assembly reference to Windows Devices or the IAsync type when building the project solution for the first time, then complete the following:

1.	Navigate to the “Uninstall a Program” window from the Start menu.
2.	Select “Microsoft Visual Studio Community 2015” and click “Change”.
3.	Install the Windows 10 SDK.
4.	Open the VATRP.App project in Visual Studio.
5.	Find the References item under the VATRP.App Project.
6.	Right-click and select “Add reference”.
7.	Select the option to Browse.
8.	In the Browse popup, select the following file: C:\Program Files (x86)\Windows Kits\10\UnionMetadata\Windows.md or Windows.winmd.
9.	Make sure the Windows.md or Windows.winmd reference file is checked off, then click “OK”.
10.	Clean and rebuild the application. Any reference errors should be resolved.

### Additional Information
For more information about the VATRP, please refer to the VATRP Platform Release Documentation included in this repository. The documentation contains release notes, an installation guide, a user guide, and a test plan.
