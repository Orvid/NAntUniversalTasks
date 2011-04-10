NAntCollections
http://www.codeplex.com/NAntCollections


What is NAntCollections?
========================
NAntCollections is a library of NAnt tasks and functions which add
the ability to use collections (namely lists and dictionaries) within
NAnt scripts.  A quick example would be something like this:
	<dict-add dictionary="MyDict" key="FirstKey" value="FirstValue"/>
	<dict-add dictionary="MyDict" key="SecondKey" value="SecondValue"/>
	<echo message="The value of the first key is ${dict::get-value('MyDict','FirstKey')}"/>


Why?
========================
NAnt and NAntContrib provide an excellent list of features to create very
robust build scripts.  However, out of the box there is no way to maintain
a collection of values except by maintaining a delimited list in a property.
This is adequate for simple cases but when it is necessary to maintain large
collections or collections of collections this quickly becomes unwieldy.
The goal of NAntCollections is to bring proper collections to NAnt.


Building NAntCollections
========================
NAntCollections is coded against the .NET Framework version 2.0.
NAntCollections can be built using Visual Studio 2005. It can also
be built using the NAntCollections.build script included with the
source code.


Using NAntCollections within NAnt scripts
=========================================
In order for a NAnt script to be able to use the tasks and functions defined
in this library, NAnt must be able to locate and scan the compiled DLL.
Details on the various way to have NAnt find the library can be found in the
NAnt documentation here under "Loading custom extensions":
http://nant.sourceforge.net/release/latest/help/fundamentals/tasks.html


Copyright and Licensing
========================
Copyright © 2007 Justin Kohlhepp

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.
 
You should have received a copy of the GNU Lesser General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301 USA