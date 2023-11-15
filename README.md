# BoxerAI
BoxerAI is a Unity-based project that uses reinforcement learning to train two virtual boxing agents to engage in a boxing match. This project provides an interactive and dynamic environment where agents learn to box effectively, make strategic decisions, and adapt to different scenarios.

## Demo Video
Explore the project in action: [Demo Video](https://drive.google.com/file/d/12G3z1rOSaekJT9X4y-fugLXYbIy_zyG5/view?usp=sharing)

## Running the Project Locally

### Prerequisites

- Unity Hub
- Unity Editor version 2022.3.7
- ML-Agents [3.0.0-exp.1](https://github.com/Unity-Technologies/ml-agents/releases)
  - Follow the [official guide](https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Getting-Started.md) for setup.

### Installation Steps

1. Download and clone this repository to your local system.
2. Install Unity Hub and download Unity Editor version 2022.3.7.
3. Download ML-Agents [3.0.0-exp.1](https://github.com/Unity-Technologies/ml-agents/releases) and follow the setup guide.
4. Create a new Unity project.
5. Copy the folders (Assets, Config, and Results) from this repository to the project folder, replacing any existing files.
6. Add ML-Agents and ML-Agents Extensions to your project:
   - Open Unity Editor.
   - Navigate to **Window -> Package Manager -> "+" -> Add package from disk.**
   - For ML-Agents, select `com.unity.mlagents` from the ml-agents-release_21 folder.
   - For ML-Agents Extensions, select `com.unity.mlagents-extensions`.
   - Double click the JSON files to add the packages.
7. Project setup is complete.
8. Play the Unity project to witness the virtual boxing match between two trained agents.

Your system is now ready to experience the BoxerAI project!

