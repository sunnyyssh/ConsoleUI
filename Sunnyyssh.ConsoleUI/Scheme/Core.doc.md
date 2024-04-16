<h1>Core</h1>

You can always write me in telegram if you have some questions about the core. I will be glad to explain you.
<a href="https://t.me/sunnyyssh">
    <img src="https://www.svgrepo.com/show/452115/telegram.svg" width="15" alt="Tg-icon"/> 
    <u><b>My tg account</b></u>
</a>

I will try to explain you how this all works. I want you to discover a source code to have a better understanding. Actually, if you can't understand write me, please.

<h2>Main idea of the core.</h2>
The idea of the Core design is in making it so abstract as it's needed to have many opportunities in specific implementations. Of course, it causes many difficulties
Also core doesn't use any low-level platfrom-specific code, that's why I use only Console class' methods to operate with console.
And higher level I created the farther from Console class I was getting. Actually, only two classes operate with console directly: KeyListener and DrawerPal.

The application's process has two parts: its building and its running.

<h2>Application building.</h2>

I decided to make application not resizable, so UI is not resize-resistant. Once it is built, its size can't be changed. 
<br/>
Also I made everything support relational sizing and positioning that's why before application runs all absolute sizes and positions must be resolved.
<br/> First off, we know actual building Application's size. Then we can resolve its children's sizes. Then if children consist of othere elements their absolute size can be resolved, and so on.
<br/> This process of resolving sizes can be easily implemented by building-mechanism. That's why every UIElement type has its own builder type (Watch <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/UIElement/IUIElementBuilder.cs">IUIElementBuilder.cs</a>).
<br/> The builder has Size property containing absolute or relational size. 
<br/> Every builder can be added to some another builders (for example to any Wrapper's builder). Then the absolute size of building element is resolved and its instance is created by its builder.
<br/> So, the application is built by its children's builders' Build method invocations. (Its built like propagating building in a tree with a root-ApplcationBuilder).
<br/> (You can make your own builder and place breakpoint into its Build methos. Then watch stack-trace and you will see Build invocations chain).


<h2>Application running</h2>

The Application instance handles all the processes of UI. Its source code is in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/Application/Application.cs">Application.cs</a>
**The main proccesses of UI**:
- Key listening. 
- Key handling and focus flow. 
- Drawing.

As you may notice, this looks not so complicated so here we go.

<h3>Key listening.</h3>
It is handled by KeyListener instance. Its source code is in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/FocusFlow/KeyListener/KeyListener.cs">KeyListener.cs</a>
<br/>
It just runs in its own (not background) thread and invokes event when key is pressed. 
<br/>
Key press is handled in this thread so current handler must handle time-expensive operations in another thread. (Watch focus flow topic to find out details).

<h3>Key handling and focus flow.</h3>
To be honest, it is the most complicated part of the library.
<br/>
The focus flow is handled by **FocusFlowManager** . Its source code is in <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/FocusFlow/FocusFlowManager/FocusFlowManager.cs">FocusFlowManager.cs</a>
<br/>
Under focus flow I mean the assignment of ability to handle keys to the IFocusable children.
<br/>

**The model of focus flow is described by these points**:
- Only elements implementing IFocusable can handle keys.
- Only one IFocusable can handle key at any moment. So, one key may not be handled by many elements.
- Wrappers have their own FocusFlowManager. So, it manages the focus flow of its IFocusable children. Watch <a href="Wrapper.doc.md">Wrapper</a>
- Application has its head FocusFlowManager.
- Focus flow of any FocusFlowManager is specified by the FocusFlowSpecification. <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/FocusFlow/FocusFlowManager/FocusFlowSpecification.cs)">Its source code.</a>
- One FocusFlowManager instance can delegate focus flow to another instance. (It is called _successor in the source code of FocusFlowManager). If _successor doesn't override focus flow then some keys are still handled by its owner (keys that causes focus change). 

**What FocusFlowSpecification describes**:
- What IFocusable children take part in focus flow.
- Every IFocusable has its ChildSpecification which describes switching focus on the specified keys to the other children. Watch <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/FocusFlow/FocusFlowManager/ChildSpecification.cs">ChildSpecification.cs</a>
- It specifies if this FocusFlowManager instance overrides flow. If it does then all keys except SpecialKey are handled by this instance bypassing manager-owner.

**The problems that can occur:**
1. Some IFocusable implentations need specific keys to handle. 
<br/>And the owner of this instance (some Wrapper or Application itself) has same keys that switch focus to another element.
<br/>(For example, it's better to make ViewTable change cell by arrows). 
<br/> As you should notice, it causes conflict. One of them won't handle specific key if it is waited by both.
<br/> It can be resolved only by specifying different keys to conflicting instances. 
<br/>(Almost every IFocusable's builder has needed properties and I suggest you implement builders with ability to specify keys).
2. Key handling proceeds in the same thread as KeyListener listens keys. It's like that because of KeyListener implementation.
<br/>
Period between key presses may be down to 5-10ms (It's only approximately). 
<br/> It's enough for time-cheap operations. But if key is handled in KeyListener thread longer than period between key presses then the next key be handled much later then its pressed. 
(Also after key which is pressed later the next pressed one may be and it will be handled even later).
<br/> That's why key handling must be safe and every IFocusable must guarantee that key will be handled almost immediately.
<br/> In most cases IFocusable's own operations aren't so expensive and the only danger is in invocations of some events that have subscribers able to run time-expensive operations.
<br/> That's why almost every event is handled parallely (except handlers that added as time-cheap, you can add ithem with UnsafeRegisterXxxHandler methods). Watch <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/UIElements/Handlers/Handler.cs">Handler.cs</a>)

<h3>Drawing.</h3>
Drawing is handled by Drawer. <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/Draw/Internal/Drawer.cs">Its source code.</a>
<br/>
Drawer queues requests to draw something. Drawer proceeds in its own non-background thread. In this thread it waits for any requests if queue is empty and draws if there are any requests.
<br/>
When Drawer decides to draw requests it combines them into one request and delegate its actual drawing to DrawerPal instance (<a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/Draw/Internal/DrawerPal.cs">Its source code.</a>).
<br/> Every request is presented by DrawState instance (Watch <a href="https://github.com/sunnyyssh/Sunnyyssh.ConsoleUI/blob/master/Sunnyyssh.ConsoleUI/Core/Draw/DrawState.cs">DrawState.cs</a>).
<br/> DrawerPal split DrawState instance to lines. And lines are splitted to one-colored parts. 
<br/> I decided not to use ANSI text colors and customization so it causes some perfomance problems. But I can make you sure that DrawerPal's drawing is optimized as it possible with given Console class' methods.

So, let's look at drawing some level higher.
<br/> Every UIElement is presented by its DrawState instance. This instance can be changed and UIElement's UI visualisation can be redrawn. Watch <a href="UIElement.doc.md">UIelement</a>)
<br/> If some elements consist of other ones then they must handle children's redraw event. (Wrapper, for example).
<br/> When Application instance starts running it is drawn and every element's DrawState instance is requested. 
<br/> Then every drawn element can be redrawn. When redraw event occurs application queues request to its running Drawer instance.
