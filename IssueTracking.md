# Introduction #

Google code comes with it's own issue tracker. We can use this to enter and see what tasks need to be done, prioritize, and see who's working on what. In addition, it will also keep track of tasks we complete, so we can easily keep track of what progress we've made week-to-week, as well as see who's slacking :P

If you work on something related to the project, please add an issue (if one doesn't already exist), set its Status to Started and add an `Assigned-*` label for your name so we know it's being worked on, and who's working on it.

# Status / Labels #

These are the statues and labels which will be helpful to familiarize yourself with:

## Status ##

The status of the issue. Use the Accepted status for issues which can be worked on immediately, and New for issues that shouldn't be worked on quite yet (group needs to agree on details, etc.).

The issue list is sorted by default so Accepted statuses are on top.

**Open Statuses:**
| **Status** | **Description** |
|:-----------|:----------------|
| New      | Issue has not had initial review yet |
| Accepted | Problem reproduced / Need acknowledged |
| Started  | Work on this issue has begun |

**Closed Statuses:**
| **Status** | **Description** |
|:-----------|:----------------|
| Fixed     | Developer made source code changes, QA should verify |
| Verified  | QA has verified that the fix worked |
| Invalid   | This was not a valid issue report |
| Duplicate | This report duplicates an existing issue |
| WontFix   | We decided to not take action on this issue |
| Done      | The requested non-coding task was completed |

## Type ##

The type of issue. New features are Enhancements, bugs are Defects.

| **Label** | **Description** |
|:----------|:----------------|
| Type-Defect      | Report of a software defect |
| Type-Enhancement | Request for enhancement |
| Type-Task        | Work item that doesn't change the code or docs |
| Type-Review      | Request for a source code review |
| Type-Other       | Some other kind of issue |

## Priority ##

In general, the higher priority, the more things that depend on it, the sooner it should be worked on, etc. The issue tracker list, by default, is sorted by highest Priority first (after Status).

| **Label** | **Description** |
|:----------|:----------------|
| Priority-Critical | Must resolve in the specified milestone |
| Priority-High     | Strongly want to resolve in the specified milestone |
| Priority-Medium   | Normal priority |
| Priority-Low      | Might slip to later milestone |

## Component ##

What area of the project the issue is related to. I've added `Component-Module` for modules, `Component-Backend` for the generator framework, and `Component-Frontend` for the rendering code. The others are default labels and not necessarily areas we need to be concerned with (end-user documentation, etc.).

| **Label** | **Description** |
|:----------|:----------------|
| Component-Module     | Related to a content generator module |
| Component-Backend    | Related to the procedural generator framework |
| Component-Frontend   | Related to the UI or OpenGL rendering code |
| Component-UI         | Issue relates to program UI |
| Component-Logic      | Issue relates to application logic |
| Component-Persistence | Issue relates to data storage components |
| Component-Scripts    | Utility and installation scripts |
| Component-Docs       | Issue relates to end-user documentation |

## Assigned ##

Label denoting who is assigned to the issue. Multiple labels of this type could be applied to an issue.

| **Label** | **Description** |
|:----------|:----------------|
| Assigned-Alec    | This issue has been assigned to Alec Emmett |
| Assigned-Beccah  | This issue has been assigned to Beccah MacKinnon |
| Assigned-Bobby   | This issue has been assigned to Bobby Crumley |
| Assigned-Charlie | This issue has been assigned to Charlie Welch |
| Assigned-Matthew | This issue has been assigned to Matthew Orlando |