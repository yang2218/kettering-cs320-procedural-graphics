# Front-end Classes #

## `Renderer` ##
Members:
  * Collection of `Scene`
  * Collection of `Viewport`
Main class for the front-end. This class is responsible for managing the GL window, and signaling viewports to draw themselves. If implemented, it could also dispatch events such as user input.

## `Scene` ##
Members:
  * Collection of `Entity`
Simply a container of objects which can be rendered in 3D. If we make sure `Entity` is serializable, we could save/load a `Scene` from file.

## `Viewport` ##
Members:
  * `Scene` to be rendered
  * Projection (camera)/"window" to draw the scene from
  * Perhaps other parameters to control how/what is drawn from the scene
Represents a viewport (portion of the screen).

## `Entity` ##
Members:
  * Transform (position, rotation, scale)
  * Id (string)
  * (others...)
Base class for any 3D object (or UI element). If we enforce subclasses to be serializable, a `Scene` can be saved and loaded from file.

# Front-end Code Flow #

The `Renderer`, as mentioned, is the master class. Drawing looks like this:

  1. `Renderer` iterates through each `Viewport` and calls its `Draw` method.
  1. `Viewport` sets up the projection, then calls the `Scene`'s `Draw` method.
  1. `Scene` iterates through its `Entity` collection and calls each `Entity`'s `Draw` method.
  1. `Entity` (the base class) saves some GL states (TBD) and applies the entity's transform, then calls the `OnDraw` virtual method.
  1. The subclass of `Entity` performs whatever drawing operations and returns.
  1. `Entity` restores some GL states (TBD) and unapplies the entity's transform. Returns.
  1. `Scene` finishes iterating through its `Entity` collection. Returns.
  1. `Viewport` does... not sure if it needs to do anything. Returns
  1. `Renderer` finishes iterating through its `Viewport` collection. Swaps the buffer.

If implemented, UI events could be dispatched similarly:

  1. `Renderer` decides which `Viewport` to dispatch event to. For example, in the case of mouse clicks, it dispatches to the `Viewport` under the mouse.
  1. `Viewport` either decides to intercept the event and handle it, or decide what `Entity` to pass it to (such as, the `Entity` under the mouse.)
  1. `Entity` either handles the event or doesn't.

# Bridging the Back-end to the Front-end #

Modules will be added to facilitate the creation of an `Entity`, modification of its properties, and addition to a scene.

One possible approach is to make `Entity` a subclass of `Module`, and add a `Module` which accepts an Entity input parameter, which will add the `Entity` to the scene. Depending on how much more interactivity we want between the back-end and the front-end, this approach will either suffice or start to break down.

## Possible extension to Back-end ##

To facilitate interactivity if UI events are implemented, a special type of `Module` called a `Trigger` can be created. The presence of a `Trigger` in a module graph allows the graph to be run automatically if the `Trigger`'s condition is fulfilled (such as a timer, or mouse click or keyboard press). The `Trigger` can have module output parameters, such as what key was pressed or what `Entity` was clicked.