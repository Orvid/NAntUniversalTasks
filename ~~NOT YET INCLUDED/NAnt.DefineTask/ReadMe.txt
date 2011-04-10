You write a new task like this:

<define name="echo3">
  <echo message="${this.message}"/>
  <echo message="${this.message}"/>
  <echo message="${this.message}"/>
</define>

...and then you call it like this:

<echo3 message="Hello World" />

Any parameter (e.g. 'message') passed to the defined task is available as (e.g.) this.message inside the defined task. I've found it useful when you don't want to write a task in C#, perhaps because all you're doing is calling a bunch of other NAnt tasks.
