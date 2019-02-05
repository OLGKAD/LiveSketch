# LiveSketch

Code structure. 

The code is organized into 7 main files. 

1. "Main.cs". \\
2. "ConstraintPoints.cs": Governs the behavior of the handles. \\
3. "ScaleFreeConstruction.cs": Performs the first step of the algorithm (prevention of the non-uniform stretching and shearing). \\
4. "ScaleAdjustment.cs": Part I of the second step of the algorithm (adjusting the scale).\\
5. "ScaleAdjustment2.cs": Part II of the second step of the algorithm (fixing the rotation). \\
6. "Util.cs": Contains some helper functions (mostly for matrix computations) used in all the other files. \\
7. "ButtonControls.cs": The UI contains several buttons (e.g. "mark an interest point", "extract motion", "transfer motion", etc.). This file governs their behavior. All the C++ functions from the GraphTrack plugin are imported and called here. \\

Each of the files "ScaleFreeConstruction.cs", "ScaleAdjustment.cs", and "ScaleAdjustment2.cs" contains two functions: InitialComputations() and Step(). InitialComputations() computes some of the values (matrices) which are fixed for a given mesh, and only need to be computed once, in the beginning (notably $G'B$, $F$, $F'$, $H'$, $D$). Step() computes some values (matrices) which depend on the current position of the handles (notably, £V'£, fitted triangles, $V''$). All Step() functions are executed on MouseDrag(), every time a handle is moved. \\
