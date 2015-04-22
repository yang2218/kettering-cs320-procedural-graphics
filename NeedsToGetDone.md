Admittedly I nor anyone else really use the issue tracker, and I don't think we have a clear idea of what's left to be done. Hopefully this list will help.

Feel free to add anything missing.

# Back End #

  * Making sure modules integrate together
  * A simple driver to show something for Stanchev this Wednesday

# Front End #

Many of these tasks have either stubs or a base class to get people started. I think I've taken care of how the front-end and back-end interacts, so the GUI-related tasks left should be fairly simple and relatively self-contained.

  * GUI Controls for the module graph
    * Mostly done, Matt to finish up
    * ColorPickerEditorControl (I believe Charlie was going to try)
    * A sidebar or other UI element for adding a new ModuleNode to a graph
    * Other GUI controls for the rest of the window: toolbar, etc.

  * The output
    * Code for rendering the final "terrain"/fractal
      * Mostly done, coloring feature and possibly normal calculation still need to be done.
    * Camera control (panning/rotating/etc)
      * I believe Ethan is working on this