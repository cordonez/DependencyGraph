# Asset dependency graph
A editor tool for the engine Unity3D that shows in a visual way the dependencies between assets.

This tool will show in a graph the references and dependencies between all the assets in your project. If you ever wondered who is using a texture, material, prefab, etc... now its easier than ever without having to check manually!

# How to use
Right click on an assets and select 'Show dependency graph'

![](https://i.imgur.com/83JdIf2.png)

A new window will appear that will highlight the current selected asset.
![](https://i.imgur.com/pT9bGzp.png)

# How to download:

1. Using the package manager -> Add the following dependency to your 'manifest.json': 
    "com.cordonez.dependencygraph": "https://github.com/cordonez/DependencyGraph.git"
    
2. Clone or download the repository into your 'Assets' folder.

# Notes

All the information will be saved inside the 'Library' folder as 'DependencyGraph.json'. Scanning big proyects can take a while so having the collection cached is very useful to save time.

It will save to the file when Unity exit, so if Unity crashes expect some inconsistency in the data. Rescanning the project will solve it.

Any issue or improvement is welcome! :)
