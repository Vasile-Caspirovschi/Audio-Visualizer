# Musializer
Audio visualizer using NAudio and Raylib-cs in C# with sound card capture.

# Below is a demo gif
The visual quality may seem to create the illusion of occasional freezing. However the app works just fine.
![Musializer Gif](https://github.com/Vasile-Caspirovschi/Musializer/assets/97791123/c41f5805-0837-496c-ab22-1e322e6cdf02)

# Stagnation
Currently I don't know how to increase the magnitude of the low frequences, they're a bit small and because of that i scaled a bit the frequences by extractiong the square root and multiplying it by 0.7f. It provedes the most pleasent visualization among the all attempts I've tried. 
