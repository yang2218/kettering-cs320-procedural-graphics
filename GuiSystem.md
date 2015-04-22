# Introduction #

We're using OpenTKGUI, an MIT-licensed GUI library over OpenTK (how convenient!), found here: https://github.com/dzamkov/OpenTKGUI. There is also a download LotsOfControls.zip which contains an exe you can run to see a demo app of some of the controls.

I've put a build of the DLL from their latest source code into our project. Unfortunately, there's not much documentation, so you may want to use git to clone their repository so you can look at the source code yourself, if you're curious.

# A short intro to OpenTKGUI #

## Controls ##

`Control` is the base class for all gui components. It can basically draw itself and receive gui events. A `Control` is either a basic element like a text box or button, or a "container" which contains other controls or containers, in a simple layout. A complex gui is simply made of these basic controls and simple layouts (often layouts nested within layouts). In OpenTKGUI, the container is responsible for giving its child components a size and (relative) position.

Here is a list of the `Control` subclasses defined OpenTKGUI:
  * Controls:
    * Blank
    * Button
    * Checkbox
    * Form (looks like a window, and is meant to be added to a LayerContainer)
    * Label
    * Menu
    * Pane
    * Popup (as in context/right-click/popup menus)
    * Progressbar
    * Scrollbar
    * Textbox
  * Containers:
    * AlignContainer
    * BorderContainer (like Java's BorderLayout)Container
    * FlowContainer
    * ManualContainer
    * ScrollContainer
    * SplitContainer
    * SunkenContainer
    * VariableContainer
    * WindowContainer

## Events ##

At regular intervals an update "pulse" is sent down the control hierarchy, which then test for changes in the input, such as mouse position, key presses/releases, etc.. A control can also set itself as the owner of keyboard and/or mouse focus, test to see if it is still the owner, and release ownership; this is really just an "ownership" notion and doesn't affect how other controls query keyboard or mouse state.


# Our GUI Architecture #

The idea right now is to have a split-pane styled window, where one side of the split shows a 3D view of the output, and the other pane shows the module graph which generated it.

The root control of the module graph view is `ModuleGraphControl`, which contains many `ModuleNodeControl`s. The `ModuleNodeControl`s in turn contain a label for the type of module it is, a textbox for a string ID, and `EditorControl`s for each of its parameters, if available. On either side of each parameter on the edge of node are "bubbles" depending on the direction of the parameter (input or output); connections can be drawn by the user between these bubbles.

I've taken a rough MVP (Model-View-Controller)-like approach to architecting this. The Control classes (the View and Controller) don't keep any state directly related to the module nodes or graph themselves, but instead modify the corresponding class in Back\_end/ModuleGraph.cs (the Model). The Control classes in turn observe changes in model classes.