# Webcam-Py-Unity-Integration
(Unity + Python Integration)

A lightweight computer vision pipeline that runs a real-time background-learning cloaking algorithm using a laptop webcam, processing the data in Python and streaming the video feed directly into a Unity UI canvas layout.

Technical Specifications
Unity Version: Unity 6 (or modern URP-compatible versions)

Python Version: Python 3.9 – 3.11 (64-bit recommended)

Required Python Libraries
Run the following command in your terminal/command prompt to install the necessary dependencies before launching the project:

Bash
pip install opencv-python numpy mediapipe
Initial Project Configuration
[!IMPORTANT]

Directory Configuration Required: Before pressing Play in Unity, you must update the script file directory paths. Select the Python_Manager object in the Unity Hierarchy and paste your exact absolute PC file path (e.g., C:\YourProject\detector.py) into the Python Script Path inspector field.

Setup & Hierarchy Structure
To recreate or verify the system layout, ensure your components are organized exactly as follows:

Unity Hierarchy Setup:

Create an empty GameObject named Python_Manager.

Attach both the PythonRunner.cs and WebcamReceiver.cs scripts to this object.

Create a UI Canvas view containing a Raw Image component. Set its dimensions to 480 x 360 (or scale evenly, e.g., 960 x 720).

Wiring Components:

Drag the Raw Image object from the hierarchy and drop it into the empty Display UI reference slot on the WebcamReceiver component.

Correcting the Image Orientation:

OpenCV and Unity map pixel spaces inversely. Select your Raw Image, look at its Rect Transform, and flip the signs of your local scale variables to negative values (e.g., set Scale X to -1 and Scale Y to -1) to correct any mirrored or upside-down orientations.

Execution Workflow
Open your Unity project and click the Play button.

The Launch Sequence: Unity immediately spins up an external background command prompt execution instance.

Window Management: The external Python webcam preview window will pop up in front of your editor screen layout first.

Activating the Scene: Simply minimize or move the Python window to reveal your Unity Editor layout. Click directly inside the Unity Game View panel to grant the engine operating system focus, and your real-time background-learned cloaking feed will display instantly on your Canvas layer!
