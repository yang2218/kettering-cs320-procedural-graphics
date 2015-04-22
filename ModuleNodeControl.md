# Introduction #
A `ModuleNodeControl` represents a node in the module graph. The graph is a visualization of the sequence of modules which generate the desired output; each node represents an instance of a module in that sequence, and serves as the interface for the user to change any values in that module's parameters or connect those parameters to other modules' parameters.

# Content Layout #
The `ModuleNodeControl` class extends Form, which allows it to be put into a LayoutContainer (the ModuleGraphControl). In order to add a complex GUI inside, you add a hierarchy of controls, using the nested layouts to simplify calculating where every child control is positioned.

> _Ignore the coloring, I'm using a code block below which tries to guess a programming language and apply syntax highlighting_

```
ModuleNodeControl
|                                   _____________________________
+-FlowContainer (Vertical)         /(ModuleNodeControl)          \
  |                                | .-------------------------. |
  +-Label (module type)            | |(Vertical FlowContainer) | |
  |                                | |  .-------------------.  | |
  +-Textbox (node ID)              | |  |Module Type (Label)|  | |
  |                                | |  `-------------------'  | |
  +-FlowContainer (Horizontal)     | |   .-----------------.   | |
  . |                              | |   |Node ID (Textbox)|   | |
  | +-"BubbleControl"              | |   `-----------------'   | |
  | | (Connector endpoint, may     | | .---------------------. | |
  | | or may not appear depending  | | |(Horizontal Flow)    | | |
  | | on whether the parameter     | | |.---..---------..---.| | |
  | | is an input parameter        | | ||Bbl||ParamName||Bbl|| | |
  | |                              | | |`---'`---------'`---'| | |
  | +-Label (parameter name)       | | `---------------------' | |
  | |                              | |    .---------------.    | |
  | +-"BubbleControl"              | |    |EditorControl 1|    | |
  |   (if parameter is an output)  | |    `---------------'    | |
  |                                | | .---------------------. | |
  +-EditorControl                  | | |(Horizontal Flow)    | | |
  . (This is an abstract class;    | | |.---..---------..---.| | |
  | the instance which is inserted | | ||Bbl||ParamName||Bbl|| | |
  | here depends on the type of    | | |`---'`---------'`---'| | |
  | the parameter (don't worry     | | `---------------------' | |
  | about figuring out that part,  | |    .---------------.    | |
  | there's a simple call which    | |    |               |    | |
  | will generate an instance for  | |    |EditorControl 2|    | |
  | you). Note some times won't    | |    |               |    | |
  | have an editor, so this may    | |    `---------------'    | |
  | be omitted)                    | `-------------------------' |
  |                                \_____________________________/
  .
  .(repeat last two for each
  . module parameter)
```