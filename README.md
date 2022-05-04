# Hand Speed

A plugin for OTD that displays your hand speed and total distance traveled. The data displayed via web and can be easily used inside OBS by adding a browser element. 

You can also access the webhook data directly, via ws://{url}:{port}/data (url/port can be found in the settings).

Total distance travelled is saved to a text file every second, which is created next to wherever OTD is. The file `global_distance_raw.txt` contains the distance in mm, `global_distance_formatted.txt` contains the formatted version (e.g. 7.3km).

All of the web CSS is customizable, and it will hot reload whenever you save/apply them in filter settings. 

## Installation

Download the .zip file in releases and drag it into the plugin manager window in OTD (plugins -> open plugin manager).

## Demo

https://user-images.githubusercontent.com/54062686/166842437-aabf44f3-ce32-48df-a60a-926e3c367578.mp4

## Customizable Options 

![image](https://user-images.githubusercontent.com/54062686/166842720-5dfde4ef-92dc-46d4-8277-0a7d26e733b0.png)
