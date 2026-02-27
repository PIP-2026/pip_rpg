# pip_rpg
The repository containing the unity part of the project. 

## Overview
+ Authors: Maria Lindling, Christof Kloninger
+ Tools: Unity 6, Node.js

## Prerequisites
- nodejs with the modules `mysql2`, `express`, and `express-validation`.
- Xampp with the database found at `/????/pip_rpg.sql` imported.
- Unity Editor version `6000.0.62f1`

## Get Started
1. Clone the repository
2. Start xampp - MySQL
3. Import the database
4. Run *\projectFileYouUse\Assets\Code\Node\app.js
   1. If everything is set up correctly, it should now listen to WebRequestCalls
   2. If not, reimport node-modules (see Prerequisites), use npm in the terminal you trust
5. In Unity, start the application in Scene: Showcase
   1. *\projectFileYouUse\Assets\Scenes\Showcase
6. To see what it does, try the Save/Load rider
   1. Select a Slot you wish to save to.
   2. Press Save, it automatically stores your data locally and synchronizes with the Database
   3. If the Session Id matches with another in the Database, it is your save and synchronized accordingly.
   4. Delete is affecting the entire slot, resetting that save slot for a new Session or the Session you are currently running
   5. Select the File button in Save/Load to view your Data, Save/Load/Delete specific Data Files within Slots
      
## Feautures
+ Stateless REST API
+ MonoBehaviour Based modular Api, Unity friendly
+ Local Datamanagement
+ Scalable Approach
+ File Management Interface
+ Modular API
