<h1>Clusters.NET</h1>

<p>
This is the Windows.NET-component of
<a href="http://github.com/svenbieg/clusters">Clusters</a>
used for ordering and sorting.<br />
<img src="https://github.com/svenbieg/Clusters.NET/assets/12587394/4ea7ffb9-b870-47e7-a20f-899c64e9b1ec" /><br />
It has some advatages compared to the default List and Dictionary.<br />
The List may be a lot faster in some use-cases because it is not an array,<br />
and items in an Index or Map can be looked up by near-by values.<br />
<br />
There is a package available at
<a href="https://www.nuget.org/packages/Clusters">nuget.org</a>
<br />

<h2>Different Clusters</h2><br />

<table>
  <tr>
    <td>[..]</td>
    <td><b>List</b><br />Items can be inserted and removed at random positions.</td>
  </tr><tr><td></td></tr><tr>
    <td>[123]</td>
    <td><b>Index</b><br />Items are sorted and can only be added once.</td>
  </tr><tr><td></td></tr><tr>
    <td>[x:y]</td>
    <td><b>Map</b><br />Items are sorted and linked with values.</td>
  </tr>
</table><br />

<br /><br /><br /><br /><br />
